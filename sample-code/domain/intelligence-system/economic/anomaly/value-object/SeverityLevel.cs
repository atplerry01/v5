namespace Whycespace.Domain.IntelligenceSystem.Economic.Anomaly;

public sealed record SeverityLevel(string Value)
{
    public static readonly SeverityLevel Low = new("low");
    public static readonly SeverityLevel Medium = new("medium");
    public static readonly SeverityLevel High = new("high");
    public static readonly SeverityLevel Critical = new("critical");

    public bool IsCritical => this == Critical || this == High;
}
