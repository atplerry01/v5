namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Computed risk score (0–100) for an identity.
/// Higher = riskier. Deterministic for same inputs.
/// </summary>
public sealed record RiskScore
{
    public decimal Value { get; }
    public string Severity { get; }

    public RiskScore(decimal value)
    {
        Value = Math.Clamp(value, 0m, 100m);
        Severity = Classify(Value);
    }

    public static RiskScore None => new(0m);

    private static string Classify(decimal score) => score switch
    {
        >= 80m => "Critical",
        >= 60m => "High",
        >= 40m => "Medium",
        >= 20m => "Low",
        _ => "Negligible"
    };

    public override string ToString() => $"{Value:F2} ({Severity})";
}
