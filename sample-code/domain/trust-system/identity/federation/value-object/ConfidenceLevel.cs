namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Confidence in a federation link (0.0–1.0 bounded).
/// Enforces invariant: value must be within [0, 1].
/// </summary>
public sealed record ConfidenceLevel
{
    public decimal Value { get; }

    public ConfidenceLevel(decimal value)
    {
        if (value < 0 || value > 1)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"ConfidenceLevel must be between 0 and 1, got {value}.");
        Value = value;
    }

    public static ConfidenceLevel Low => new(0.25m);
    public static ConfidenceLevel Medium => new(0.5m);
    public static ConfidenceLevel High => new(0.75m);
    public static ConfidenceLevel Full => new(1.0m);

    public override string ToString() => Value.ToString("F2");
}
