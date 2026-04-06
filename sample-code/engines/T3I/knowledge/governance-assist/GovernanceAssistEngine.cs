using Whycespace.Engines.T3I.GovernanceAssist;
using Whycespace.Engines.T3I.PolicySimulation;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.Knowledge.GovernanceAssist;

/// <summary>
/// T3I engine: Autonomous Governance Assist (AGA).
/// Pipeline: input → InsightExtractor → RecommendationGenerator → BiasDetector →
///           GuardrailSpecification → TrustScorer → OptimizationEngine →
///           RecommendationSetEvaluator → ExplanationBuilder → Result.
/// Read-only, advisory-only, deterministic. NEVER modifies policy state.
/// No repository dependencies — accepts all data as parameters.
/// </summary>
public sealed class GovernanceAssistEngine
{
    private readonly RecommendationGenerator _recommendationGenerator;
    private readonly InsightExtractor _insightExtractor;
    private readonly OptimizationEngine _optimizationEngine;
    private readonly PolicyDraftGenerator _draftGenerator;
    private readonly PolicyTrustScorer _trustScorer;
    private readonly RecommendationBiasDetector _biasDetector;
    private readonly GovernanceGuardrailSpecification _guardrailSpec;
    private readonly RecommendationExplanationBuilder _explanationBuilder;
    private readonly RecommendationSetEvaluator _setEvaluator;
    private readonly IClock _clock;

    public GovernanceAssistEngine(
        RecommendationGenerator recommendationGenerator,
        InsightExtractor insightExtractor,
        OptimizationEngine optimizationEngine,
        PolicyDraftGenerator draftGenerator,
        PolicyTrustScorer trustScorer,
        RecommendationBiasDetector biasDetector,
        GovernanceGuardrailSpecification guardrailSpec,
        RecommendationExplanationBuilder explanationBuilder,
        RecommendationSetEvaluator setEvaluator,
        IClock clock)
    {
        _recommendationGenerator = recommendationGenerator;
        _insightExtractor = insightExtractor;
        _optimizationEngine = optimizationEngine;
        _draftGenerator = draftGenerator;
        _trustScorer = trustScorer;
        _biasDetector = biasDetector;
        _guardrailSpec = guardrailSpec;
        _explanationBuilder = explanationBuilder;
        _setEvaluator = setEvaluator;
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Task<GovernanceAssistResult> AnalyzeAsync(
        GovernanceAssistCommand command,
        IReadOnlyList<PolicySimulation.PolicySimulationResult> simulationResults,
        IReadOnlyList<RecommendationHistoryEntry> history,
        IReadOnlyList<RecommendationFeedback> feedbackHistory,
        CancellationToken cancellationToken = default)
    {
        var startTime = _clock.UtcNowOffset;

        var filtered = command.PolicyIdFilter.HasValue
            ? simulationResults
                .Where(r => r.PolicyVersionsUsed.Any(p => p.PolicyId == command.PolicyIdFilter.Value))
                .ToList()
            : simulationResults.ToList();

        var systemInsights = _insightExtractor.ExtractSystemLevel(filtered);
        var recommendations = _recommendationGenerator.Generate(filtered);

        var feedbackProcessor = new RecommendationFeedbackProcessor(_clock);
        var driftStability = feedbackProcessor.ComputeDriftStability(feedbackHistory);

        var summaries = new List<RecommendationSummary>();
        var trustScores = new List<RecommendationTrustScore>();

        foreach (var rec in recommendations)
        {
            var bias = _biasDetector.Detect(rec, history);
            var guardrail = _guardrailSpec.Evaluate(rec);
            var trust = _trustScorer.Score(rec, history, driftStability);
            var explanation = _explanationBuilder.Build(rec, trust, bias, guardrail);

            trustScores.Add(trust);

            summaries.Add(new RecommendationSummary
            {
                RecommendationId = rec.RecommendationId.Value,
                Source = rec.Source.ToString(),
                AffectedPolicies = rec.AffectedPolicies,
                SuggestedChanges = rec.SuggestedChanges,
                ConfidenceScore = rec.Confidence.Value,
                CompositeImpact = rec.Impact.CompositeScore,
                RiskScore = rec.Risk.Score,
                RiskCategory = rec.Risk.Category,
                Insights = rec.Insights.Select(i => new InsightSummary(
                    i.Type.ToString(), i.Description, i.Severity)).ToList(),
                TrustScore = new TrustScoreSummary(
                    trust.HistoricalAccuracy,
                    trust.AcceptanceRate,
                    trust.DriftStability,
                    trust.ConfidenceScore,
                    trust.CompositeScore),
                BiasDetected = bias.BiasDetected,
                BiasType = bias.BiasDetected ? bias.Type.ToString() : null,
                BiasSeverity = bias.BiasDetected ? bias.Severity : null,
                RequiresManualReview = guardrail.RequiresManualReview,
                GuardrailViolations = guardrail.Violations,
                Explanation = MapExplanation(explanation)
            });
        }

        OptimizationSummary? optimization = null;
        if (command.IncludeOptimization)
        {
            optimization = _optimizationEngine.Optimize(filtered, command.MaxRiskThreshold);
        }

        if (recommendations.Count > 1)
        {
            var setResult = _setEvaluator.Evaluate(new List<RecommendationSet>
            {
                new("Primary", recommendations, trustScores)
            });

            var ranked = summaries
                .OrderByDescending(s => (s.TrustScore?.CompositeScore ?? 0) * 0.4
                                      + s.ConfidenceScore * 0.3
                                      + (1.0 - s.RiskScore / 100.0) * 0.3)
                .Select((s, i) => s with { StrategyRank = i + 1 })
                .ToList();
            summaries = ranked;
        }
        else if (summaries.Count == 1)
        {
            summaries = summaries.Select(s => s with { StrategyRank = 1 }).ToList();
        }

        var duration = _clock.UtcNowOffset - startTime;

        return Task.FromResult(new GovernanceAssistResult
        {
            AnalysisId = command.AnalysisId,
            Recommendations = summaries,
            Insights = systemInsights,
            Optimization = optimization,
            AnalyzedAt = startTime,
            Duration = duration
        });
    }

    public Task<GovernanceAssistResult> AnalyzeAsync(
        GovernanceAssistCommand command,
        IReadOnlyList<PolicySimulation.PolicySimulationResult> simulationResults,
        CancellationToken cancellationToken = default)
    {
        return AnalyzeAsync(command, simulationResults, [], [], cancellationToken);
    }

    public IReadOnlyList<GovernanceProposalDraft> GenerateProposalDrafts(GovernanceRecommendationAggregate recommendation)
    {
        return _draftGenerator.GenerateDrafts(recommendation);
    }

    private static ExplanationTreeSummary MapExplanation(ExplanationTree tree)
    {
        return new ExplanationTreeSummary(tree.ReasoningSummary, tree.Nodes.Select(MapNode).ToList());
    }

    private static ExplanationNodeSummary MapNode(ExplanationNode node)
    {
        return new ExplanationNodeSummary(
            node.Label,
            node.Category,
            node.Detail,
            node.Children.Select(MapNode).ToList());
    }
}
