namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Computed trust score (0–100) for an identity.
/// Deterministic: same inputs always produce the same score.
/// </summary>
public sealed record TrustScore
{
    public decimal Value { get; }
    public string Classification { get; }

    public TrustScore(decimal value)
    {
        Value = Math.Clamp(value, 0m, 100m);
        Classification = Classify(Value);
    }

    public static TrustScore Zero => new(0m);

    private static string Classify(decimal score) => score switch
    {
        >= 90m => "Excellent",
        >= 75m => "High",
        >= 50m => "Medium",
        >= 25m => "Low",
        _ => "Untrusted"
    };

    public override string ToString() => $"{Value:F2} ({Classification})";
}
