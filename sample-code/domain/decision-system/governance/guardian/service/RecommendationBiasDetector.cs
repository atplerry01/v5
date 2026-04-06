namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Domain service that detects bias in recommendations.
/// Detects: repetition bias, cluster bias, outcome skew.
/// Pure domain logic — deterministic, no side effects.
/// </summary>
public sealed class RecommendationBiasDetector
{
    private const double RepetitionThreshold = 0.7;
    private const double ClusterThreshold = 0.6;
    private const double SkewThreshold = 0.75;

    /// <summary>
    /// Detect bias in a single recommendation against historical context.
    /// </summary>
    public BiasAssessment Detect(
        GovernanceRecommendationAggregate recommendation,
        IReadOnlyList<RecommendationHistoryEntry> history)
    {
        var repetitionBias = DetectRepetitionBias(recommendation, history);
        if (repetitionBias.BiasDetected) return repetitionBias;

        var clusterBias = DetectClusterBias(recommendation, history);
        if (clusterBias.BiasDetected) return clusterBias;

        var skewBias = DetectOutcomeSkew(recommendation, history);
        if (skewBias.BiasDetected) return skewBias;

        return BiasAssessment.None();
    }

    private static BiasAssessment DetectRepetitionBias(
        GovernanceRecommendationAggregate recommendation,
        IReadOnlyList<RecommendationHistoryEntry> history)
    {
        if (history.Count == 0) return BiasAssessment.None();

        var matchingSource = history.Count(h => h.Source == recommendation.Source.ToString());
        var ratio = (double)matchingSource / history.Count;

        if (ratio >= RepetitionThreshold)
            return BiasAssessment.Detected(
                BiasType.Repetition,
                "High",
                $"Repetition bias: {ratio:P0} of recent recommendations share the same source ({recommendation.Source}).");

        return BiasAssessment.None();
    }

    private static BiasAssessment DetectClusterBias(
        GovernanceRecommendationAggregate recommendation,
        IReadOnlyList<RecommendationHistoryEntry> history)
    {
        if (history.Count == 0) return BiasAssessment.None();

        var affectedSet = recommendation.AffectedPolicies.ToHashSet();
        var overlapping = history.Count(h =>
            h.AffectedPolicies.Any(p => affectedSet.Contains(p)));

        var ratio = (double)overlapping / history.Count;

        if (ratio >= ClusterThreshold)
            return BiasAssessment.Detected(
                BiasType.Cluster,
                "Medium",
                $"Cluster bias: {ratio:P0} of recent recommendations target overlapping policies.");

        return BiasAssessment.None();
    }

    private static BiasAssessment DetectOutcomeSkew(
        GovernanceRecommendationAggregate recommendation,
        IReadOnlyList<RecommendationHistoryEntry> history)
    {
        if (history.Count < 5) return BiasAssessment.None();

        var positiveImpact = history.Count(h => h.CompositeImpact > 0);
        var ratio = (double)positiveImpact / history.Count;

        if (ratio >= SkewThreshold || ratio <= (1.0 - SkewThreshold))
            return BiasAssessment.Detected(
                BiasType.OutcomeSkew,
                "Medium",
                $"Outcome skew: {ratio:P0} of historical recommendations show uniformly {(ratio >= SkewThreshold ? "positive" : "negative")} impact.");

        return BiasAssessment.None();
    }
}

/// <summary>
/// Lightweight history entry for bias detection. Not an aggregate — consumed read-only.
/// </summary>
public sealed record RecommendationHistoryEntry(
    Guid RecommendationId,
    string Source,
    IReadOnlyList<Guid> AffectedPolicies,
    double CompositeImpact,
    bool WasAccepted);
