namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Confidence score for a governance recommendation. Range: 0.0 to 1.0.
/// </summary>
public sealed record RecommendationConfidence
{
    public double Value { get; }

    private RecommendationConfidence(double value) => Value = value;

    public static RecommendationConfidence From(double value) =>
        value < 0.0 || value > 1.0
            ? throw new ArgumentOutOfRangeException(nameof(value), "Confidence must be between 0.0 and 1.0.")
            : new(value);

    public bool IsHighConfidence => Value >= 0.8;
    public bool IsLowConfidence => Value < 0.5;
}
