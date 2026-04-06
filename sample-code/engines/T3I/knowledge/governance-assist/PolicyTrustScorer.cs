namespace Whycespace.Engines.T3I.GovernanceAssist;

/// <summary>
/// Computes trust scores for recommendations using historical accuracy,
/// acceptance rates, drift stability, and confidence signals.
/// Deterministic: same inputs → same trust score.
/// </summary>
public sealed class PolicyTrustScorer
{
    /// <summary>
    /// Compute trust score for a recommendation using calibration data.
    /// </summary>
    public RecommendationTrustScore Score(
        GovernanceRecommendationAggregate recommendation,
        IReadOnlyList<RecommendationHistoryEntry> history,
        double driftStability)
    {
        var historicalAccuracy = ComputeHistoricalAccuracy(history);
        var acceptanceRate = ComputeAcceptanceRate(history);

        return RecommendationTrustScore.From(
            historicalAccuracy,
            acceptanceRate,
            Math.Clamp(driftStability, 0.0, 1.0),
            recommendation.Confidence.Value);
    }

    private static double ComputeHistoricalAccuracy(IReadOnlyList<RecommendationHistoryEntry> history)
    {
        if (history.Count == 0) return 0.5; // neutral baseline

        var accepted = history.Count(h => h.WasAccepted);
        var positiveOutcomes = history.Count(h => h.WasAccepted && h.CompositeImpact > 0);

        return accepted > 0
            ? (double)positiveOutcomes / accepted
            : 0.5;
    }

    private static double ComputeAcceptanceRate(IReadOnlyList<RecommendationHistoryEntry> history)
    {
        if (history.Count == 0) return 0.5;

        return (double)history.Count(h => h.WasAccepted) / history.Count;
    }
}
