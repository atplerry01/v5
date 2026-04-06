namespace Whycespace.Domain.IntelligenceSystem.Economic;

public sealed record ConfidenceScore
{
    public decimal Value { get; }

    public ConfidenceScore(decimal value)
    {
        if (value < 0 || value > 1)
            throw new ArgumentException("Confidence score must be between 0 and 1.", nameof(value));
        Value = value;
    }

    public static ConfidenceScore Zero => new(0);
    public static ConfidenceScore Full => new(1);

    public bool IsHighConfidence => Value >= 0.8m;
    public bool IsLowConfidence => Value < 0.3m;
}
