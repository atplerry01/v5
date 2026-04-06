namespace Whycespace.Engines.T3I.GovernanceAssist;

/// <summary>
/// Evaluates multiple recommendation sets and ranks the best strategy.
/// Deterministic: same inputs → same ranking.
/// </summary>
public sealed class RecommendationSetEvaluator
{
    /// <summary>
    /// Evaluate and rank multiple recommendation sets by composite quality.
    /// </summary>
    public StrategyComparisonResult Evaluate(IReadOnlyList<RecommendationSet> sets)
    {
        if (sets.Count == 0)
            return new StrategyComparisonResult([], null);

        var ranked = sets
            .Select(s => new RankedStrategy(
                s.StrategyName,
                s.Recommendations,
                ComputeStrategyScore(s)))
            .OrderByDescending(r => r.Score)
            .ToList();

        return new StrategyComparisonResult(ranked, ranked.First());
    }

    private static double ComputeStrategyScore(RecommendationSet set)
    {
        if (set.Recommendations.Count == 0) return 0.0;

        var avgConfidence = set.Recommendations.Average(r => r.Confidence.Value);
        var avgImpact = set.Recommendations.Average(r => r.Impact.CompositeScore);
        var avgRisk = set.Recommendations.Average(r => (double)r.Risk.Score / 100.0);
        var avgTrust = set.TrustScores.Count > 0
            ? set.TrustScores.Average(t => t.CompositeScore)
            : 0.5;

        // Higher confidence, impact, trust = better. Lower risk = better.
        return (avgConfidence * 0.25) + (avgImpact * 0.25) + (avgTrust * 0.25) + ((1.0 - avgRisk) * 0.25);
    }
}

public sealed record RecommendationSet(
    string StrategyName,
    IReadOnlyList<GovernanceRecommendationAggregate> Recommendations,
    IReadOnlyList<RecommendationTrustScore> TrustScores);

public sealed record RankedStrategy(
    string StrategyName,
    IReadOnlyList<GovernanceRecommendationAggregate> Recommendations,
    double Score);

public sealed record StrategyComparisonResult(
    IReadOnlyList<RankedStrategy> RankedStrategies,
    RankedStrategy? OptimalStrategy);
