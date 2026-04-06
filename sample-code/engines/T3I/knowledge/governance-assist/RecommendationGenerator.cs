using Whycespace.Engines.T3I.PolicySimulation;

namespace Whycespace.Engines.T3I.GovernanceAssist;

/// <summary>
/// Generates structured governance recommendations from simulation outputs,
/// drift signals, and federation conflicts.
/// Deterministic: same inputs → same recommendations.
/// </summary>
public sealed class RecommendationGenerator
{
    private readonly InsightExtractor _insightExtractor;
    private readonly GovernanceRecommendationEngine _domainEngine;

    public RecommendationGenerator(InsightExtractor insightExtractor, GovernanceRecommendationEngine domainEngine)
    {
        _insightExtractor = insightExtractor;
        _domainEngine = domainEngine;
    }

    public IReadOnlyList<GovernanceRecommendationAggregate> Generate(IReadOnlyList<PolicySimulationResult> simulationResults)
    {
        var recommendations = new List<GovernanceRecommendationAggregate>();

        var grouped = GroupByRecommendationSource(simulationResults);

        foreach (var (source, results) in grouped)
        {
            var insights = _insightExtractor.Extract(results);
            if (insights.Count == 0) continue;

            var affectedPolicies = results
                .SelectMany(r => r.PolicyVersionsUsed.Select(p => p.PolicyId))
                .Distinct()
                .ToList();

            var suggestedChanges = DeriveSuggestedChanges(source, results);
            var (confidence, economic, operational, governance, risk) = ComputeScores(results);

            try
            {
                var recommendation = _domainEngine.Generate(
                    source,
                    affectedPolicies,
                    suggestedChanges,
                    confidence,
                    economic,
                    operational,
                    governance,
                    risk,
                    insights);

                recommendations.Add(recommendation);
            }
            catch (InvalidOperationException)
            {
                // Recommendation did not meet validity spec — skip
            }
        }

        return recommendations;
    }

    private static Dictionary<RecommendationSource, List<PolicySimulationResult>> GroupByRecommendationSource(
        IReadOnlyList<PolicySimulationResult> results)
    {
        var grouped = new Dictionary<RecommendationSource, List<PolicySimulationResult>>();

        foreach (var result in results)
        {
            var source = DetermineSource(result);
            if (!grouped.ContainsKey(source))
                grouped[source] = [];
            grouped[source].Add(result);
        }

        return grouped;
    }

    private static RecommendationSource DetermineSource(PolicySimulationResult result)
    {
        if (result.Drift is { DriftDetected: true })
            return RecommendationSource.Drift;
        if (result.Anomalies.Count > 0)
            return RecommendationSource.Anomaly;
        if (result.CrossClusterConflicts is { Count: > 0 })
            return RecommendationSource.Federation;
        return RecommendationSource.Simulation;
    }

    private static List<string> DeriveSuggestedChanges(
        RecommendationSource source,
        List<PolicySimulationResult> results)
    {
        var changes = new List<string>();

        switch (source)
        {
            case RecommendationSource.Drift:
                changes.Add("Recalibrate policy parameters to reduce drift from observed outcomes.");
                break;
            case RecommendationSource.Anomaly:
                var anomalyTypes = results
                    .SelectMany(r => r.Anomalies.Select(a => a.Type))
                    .Distinct();
                foreach (var type in anomalyTypes)
                    changes.Add($"Review and address anomaly pattern: {type}.");
                break;
            case RecommendationSource.Federation:
                changes.Add("Resolve cross-cluster policy conflicts identified in federation graph.");
                break;
            case RecommendationSource.Simulation:
                var failingRules = results.Sum(r => r.DecisionSummary.RulesFailed);
                if (failingRules > 0)
                    changes.Add($"Adjust {failingRules} failing rule(s) to improve policy pass rate.");
                else
                    changes.Add("Evaluate policy for optimization opportunities based on simulation results.");
                break;
        }

        return changes;
    }

    private static (double confidence, double economic, double operational, double governance, int risk) ComputeScores(
        List<PolicySimulationResult> results)
    {
        var confidence = results
            .Where(r => r.Confidence is not null)
            .Select(r => r.Confidence!.ConfidenceScore)
            .DefaultIfEmpty(0.5)
            .Average();

        var economic = results
            .Where(r => r.NormalizedImpact is not null)
            .Select(r => r.NormalizedImpact!.EconomicScore)
            .DefaultIfEmpty(0.0)
            .Average();

        // Clamp to [-1, 1] range for ImpactEstimate
        economic = Math.Clamp(economic, -1.0, 1.0);

        var operational = Math.Clamp(results
            .Where(r => r.NormalizedImpact is not null)
            .Select(r => r.NormalizedImpact!.OperationalScore)
            .DefaultIfEmpty(0.0)
            .Average(), -1.0, 1.0);

        var governance = Math.Clamp(results
            .Where(r => r.NormalizedImpact is not null)
            .Select(r => r.NormalizedImpact!.GovernanceScore)
            .DefaultIfEmpty(0.0)
            .Average(), -1.0, 1.0);

        var risk = (int)results
            .Where(r => r.RiskScore is not null)
            .Select(r => (double)r.RiskScore!.Score)
            .DefaultIfEmpty(25)
            .Average();

        risk = Math.Clamp(risk, 0, 100);

        return (confidence, economic, operational, governance, risk);
    }
}
