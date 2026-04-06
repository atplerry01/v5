namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Trust level for an issuer (0–100 bounded).
/// Enforces invariant: value must be within [0, 100].
/// </summary>
public sealed record TrustLevel
{
    public decimal Value { get; }

    public TrustLevel(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"TrustLevel must be between 0 and 100, got {value}.");
        Value = value;
    }

    public static TrustLevel Zero => new(0m);
    public static TrustLevel Minimum => new(25m);

    public string Classification => Value switch
    {
        >= 90m => "Excellent",
        >= 75m => "High",
        >= 50m => "Medium",
        >= 25m => "Low",
        _ => "Untrusted"
    };

    public bool MeetsThreshold(decimal threshold) => Value >= threshold;

    public override string ToString() => $"{Value:F1} ({Classification})";
}
