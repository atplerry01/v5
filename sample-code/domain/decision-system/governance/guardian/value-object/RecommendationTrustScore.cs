namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Composite trust score for a governance recommendation.
/// Deterministic: same inputs → same score.
/// </summary>
public sealed record RecommendationTrustScore
{
    public double HistoricalAccuracy { get; }
    public double AcceptanceRate { get; }
    public double DriftStability { get; }
    public double ConfidenceScore { get; }
    public double CompositeScore { get; }

    private const double WeightHistorical = 0.35;
    private const double WeightAcceptance = 0.25;
    private const double WeightDrift = 0.20;
    private const double WeightConfidence = 0.20;

    private RecommendationTrustScore(
        double historicalAccuracy,
        double acceptanceRate,
        double driftStability,
        double confidenceScore)
    {
        HistoricalAccuracy = historicalAccuracy;
        AcceptanceRate = acceptanceRate;
        DriftStability = driftStability;
        ConfidenceScore = confidenceScore;
        CompositeScore = WeightHistorical * historicalAccuracy
                       + WeightAcceptance * acceptanceRate
                       + WeightDrift * driftStability
                       + WeightConfidence * confidenceScore;
    }

    public static RecommendationTrustScore From(
        double historicalAccuracy,
        double acceptanceRate,
        double driftStability,
        double confidenceScore)
    {
        Validate(historicalAccuracy, nameof(historicalAccuracy));
        Validate(acceptanceRate, nameof(acceptanceRate));
        Validate(driftStability, nameof(driftStability));
        Validate(confidenceScore, nameof(confidenceScore));
        return new(historicalAccuracy, acceptanceRate, driftStability, confidenceScore);
    }

    public bool IsHighTrust => CompositeScore >= 0.7;
    public bool IsLowTrust => CompositeScore < 0.4;

    private static void Validate(double value, string name)
    {
        if (value < 0.0 || value > 1.0)
            throw new ArgumentOutOfRangeException(name, $"Trust component must be between 0.0 and 1.0.");
    }
}
