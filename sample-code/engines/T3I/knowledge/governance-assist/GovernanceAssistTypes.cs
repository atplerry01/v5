using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Engines.T3I.GovernanceAssist;

// ─────────────────────────────────────────────────────────────────
// Engine-local replacements for domain types previously imported
// from Whycespace.Domain.DecisionSystem.Governance.Guardian.
// These are engine-scoped value objects / aggregates. No domain dependency.
// ─────────────────────────────────────────────────────────────────

#region Enums

public enum InsightType
{
    Anomaly,
    Drift,
    Inefficiency,
    Conflict
}

public enum RecommendationSource
{
    Simulation,
    Drift,
    Anomaly,
    Federation
}

#endregion

#region Value Objects

public sealed record RecommendationId(Guid Value);

public sealed record ConfidenceScore
{
    public double Value { get; }

    private ConfidenceScore(double value) => Value = value;

    public static ConfidenceScore From(double value) =>
        new(Math.Clamp(value, 0.0, 1.0));
}

public sealed record ImpactEstimate
{
    public double EconomicScore { get; }
    public double OperationalScore { get; }
    public double GovernanceScore { get; }
    public double CompositeScore { get; }

    private ImpactEstimate(double economic, double operational, double governance)
    {
        EconomicScore = economic;
        OperationalScore = operational;
        GovernanceScore = governance;
        CompositeScore = (economic + operational + governance) / 3.0;
    }

    public static ImpactEstimate From(double economic, double operational, double governance) =>
        new(
            Math.Clamp(economic, -1.0, 1.0),
            Math.Clamp(operational, -1.0, 1.0),
            Math.Clamp(governance, -1.0, 1.0));
}

public sealed record RiskEstimate
{
    public int Score { get; }
    public string Category { get; }
    public bool IsCritical { get; }

    private RiskEstimate(int score)
    {
        Score = Math.Clamp(score, 0, 100);
        Category = Score switch
        {
            > 75 => "critical",
            > 50 => "high",
            > 25 => "medium",
            _ => "low"
        };
        IsCritical = Score > 75;
    }

    public static RiskEstimate From(int score) => new(score);
}

#endregion

#region Aggregates

/// <summary>
/// Engine-local aggregate representing a governance recommendation.
/// Replaces the domain GovernanceRecommendationAggregate.
/// </summary>
public sealed record GovernanceRecommendationAggregate
{
    public required RecommendationId RecommendationId { get; init; }
    public required RecommendationSource Source { get; init; }
    public required IReadOnlyList<Guid> AffectedPolicies { get; init; }
    public required IReadOnlyList<string> SuggestedChanges { get; init; }
    public required ConfidenceScore Confidence { get; init; }
    public required ImpactEstimate Impact { get; init; }
    public required RiskEstimate Risk { get; init; }
    public required IReadOnlyList<RecommendationInsight> Insights { get; init; }
}

#endregion

#region Insights

/// <summary>
/// A single insight extracted from simulation analysis.
/// </summary>
public sealed record RecommendationInsight(
    Guid InsightId,
    InsightType Type,
    string Description,
    string Severity);

#endregion

#region Trust Scoring

/// <summary>
/// Composite trust score for a recommendation.
/// </summary>
public sealed record RecommendationTrustScore
{
    public double HistoricalAccuracy { get; init; }
    public double AcceptanceRate { get; init; }
    public double DriftStability { get; init; }
    public double ConfidenceScore { get; init; }
    public double CompositeScore { get; init; }
    public bool IsHighTrust => CompositeScore >= 0.7;
    public bool IsLowTrust => CompositeScore < 0.4;

    public static RecommendationTrustScore From(
        double historicalAccuracy,
        double acceptanceRate,
        double driftStability,
        double confidenceScore)
    {
        var composite = (historicalAccuracy * 0.3)
                      + (acceptanceRate * 0.25)
                      + (driftStability * 0.25)
                      + (confidenceScore * 0.2);

        return new RecommendationTrustScore
        {
            HistoricalAccuracy = historicalAccuracy,
            AcceptanceRate = acceptanceRate,
            DriftStability = driftStability,
            ConfidenceScore = confidenceScore,
            CompositeScore = Math.Clamp(composite, 0.0, 1.0)
        };
    }
}

/// <summary>
/// Historical record of a previous recommendation outcome.
/// </summary>
public sealed record RecommendationHistoryEntry
{
    public required Guid RecommendationId { get; init; }
    public required bool WasAccepted { get; init; }
    public required double CompositeImpact { get; init; }
    public required DateTimeOffset RecordedAt { get; init; }
}

#endregion

#region Explanation

/// <summary>
/// Hierarchical explanation tree for recommendation explainability.
/// </summary>
public sealed record ExplanationTree
{
    public required string ReasoningSummary { get; init; }
    public required IReadOnlyList<ExplanationNode> Nodes { get; init; }
}

/// <summary>
/// A single node in the explanation tree.
/// </summary>
public sealed record ExplanationNode
{
    public required string Label { get; init; }
    public required string Category { get; init; }
    public required string Detail { get; init; }
    public required IReadOnlyList<ExplanationNode> Children { get; init; }
}

#endregion

#region Optimization

/// <summary>
/// Engine-local optimization service. Selects optimal policies under risk constraints.
/// Replaces the domain PolicyOptimizationEngine.
/// </summary>
public sealed class PolicyOptimizationEngine
{
    public PolicyOptimizationResult Optimize(
        IReadOnlyList<PolicyCandidate> candidates,
        int maxRiskThreshold)
    {
        var selected = candidates
            .Where(c => c.AverageRisk <= maxRiskThreshold)
            .OrderByDescending(c => c.AverageImpact)
            .ToList();

        if (selected.Count == 0)
        {
            return new PolicyOptimizationResult([], 0.0, 0);
        }

        var avgImpact = selected.Average(c => c.AverageImpact);
        var totalRisk = (int)selected.Average(c => c.AverageRisk);

        return new PolicyOptimizationResult(selected, avgImpact, totalRisk);
    }
}

public sealed record PolicyCandidate(
    Guid PolicyId,
    double AverageImpact,
    int AverageRisk,
    string Summary);

public sealed record PolicyOptimizationResult(
    IReadOnlyList<PolicyCandidate> SelectedPolicies,
    double AverageImpact,
    int TotalRisk);

#endregion

#region Bias Detection

/// <summary>
/// Detects statistical or systematic biases in recommendations.
/// </summary>
public sealed class RecommendationBiasDetector
{
    public BiasAssessment Detect(
        GovernanceRecommendationAggregate recommendation,
        IReadOnlyList<RecommendationHistoryEntry> history)
    {
        // Check for recency bias: if most recent history entries dominate
        if (history.Count >= 5)
        {
            var recentAcceptance = history
                .OrderByDescending(h => h.RecordedAt)
                .Take(3)
                .Count(h => h.WasAccepted);

            var overallAcceptance = history.Count(h => h.WasAccepted);
            var overallRate = (double)overallAcceptance / history.Count;
            var recentRate = recentAcceptance / 3.0;

            if (Math.Abs(recentRate - overallRate) > 0.4)
            {
                return new BiasAssessment(
                    BiasDetected: true,
                    Type: BiasType.Recency,
                    Severity: "Medium",
                    Description: "Recent acceptance pattern deviates significantly from historical average.");
            }
        }

        // Check for confirmation bias: recommendations with low risk always accepted
        if (recommendation.Risk.Score < 20 && history.Count > 0)
        {
            var lowRiskAcceptance = history.Count(h => h.WasAccepted && h.CompositeImpact < 0.3);
            if (lowRiskAcceptance > history.Count * 0.8)
            {
                return new BiasAssessment(
                    BiasDetected: true,
                    Type: BiasType.Confirmation,
                    Severity: "Low",
                    Description: "Low-risk recommendations are disproportionately accepted regardless of impact.");
            }
        }

        return new BiasAssessment(
            BiasDetected: false,
            Type: BiasType.None,
            Severity: null,
            Description: null);
    }
}

public sealed record BiasAssessment(
    bool BiasDetected,
    BiasType Type,
    string? Severity,
    string? Description);

public enum BiasType
{
    None,
    Recency,
    Confirmation,
    Anchoring,
    Selection
}

#endregion

#region Guardrail

/// <summary>
/// Evaluates recommendations against governance guardrails.
/// </summary>
public sealed class GovernanceGuardrailSpecification
{
    public GuardrailResult Evaluate(GovernanceRecommendationAggregate recommendation)
    {
        var violations = new List<string>();

        if (recommendation.Risk.IsCritical)
        {
            violations.Add("Recommendation risk score exceeds critical threshold (>75).");
        }

        if (recommendation.Confidence.Value < 0.3)
        {
            violations.Add("Recommendation confidence is below minimum threshold (<0.3).");
        }

        if (recommendation.AffectedPolicies.Count > 10)
        {
            violations.Add($"Recommendation affects {recommendation.AffectedPolicies.Count} policies (>10). Broad impact requires review.");
        }

        return new GuardrailResult(
            RequiresManualReview: violations.Count > 0,
            Violations: violations);
    }
}

public sealed record GuardrailResult(
    bool RequiresManualReview,
    IReadOnlyList<string> Violations);

#endregion

#region Recommendation Engine

/// <summary>
/// Engine-local recommendation generation service.
/// Constructs GovernanceRecommendationAggregate from scored inputs.
/// Replaces the domain GovernanceRecommendationEngine used by RecommendationGenerator.
/// </summary>
public sealed class GovernanceRecommendationEngine
{
    public GovernanceRecommendationAggregate Generate(
        RecommendationSource source,
        IReadOnlyList<Guid> affectedPolicies,
        IReadOnlyList<string> suggestedChanges,
        double confidence,
        double economic,
        double operational,
        double governance,
        int risk,
        IReadOnlyList<RecommendationInsight> insights)
    {
        if (affectedPolicies.Count == 0)
            throw new InvalidOperationException("Recommendation must affect at least one policy.");

        if (insights.Count == 0)
            throw new InvalidOperationException("Recommendation must have at least one insight.");

        var seed = $"Rec:{source}:{string.Join(",", affectedPolicies)}:{confidence:F4}";

        return new GovernanceRecommendationAggregate
        {
            RecommendationId = new RecommendationId(DeterministicIdHelper.FromSeed(seed)),
            Source = source,
            AffectedPolicies = affectedPolicies,
            SuggestedChanges = suggestedChanges,
            Confidence = ConfidenceScore.From(confidence),
            Impact = ImpactEstimate.From(economic, operational, governance),
            Risk = RiskEstimate.From(risk),
            Insights = insights
        };
    }
}

#endregion
