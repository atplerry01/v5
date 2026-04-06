namespace Whycespace.Domain.DecisionSystem.Compliance.Attestation;

public sealed record RiskLevel(string Value)
{
    public static readonly RiskLevel Unknown = new("UNKNOWN");
    public static readonly RiskLevel Low = new("LOW");
    public static readonly RiskLevel Medium = new("MEDIUM");
    public static readonly RiskLevel High = new("HIGH");
    public static readonly RiskLevel Critical = new("CRITICAL");
}
