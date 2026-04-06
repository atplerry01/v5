namespace Whycespace.Domain.IntelligenceSystem.Economic.Anomaly;

public sealed record AnomalyType(string Value)
{
    public static readonly AnomalyType Fraud = new("fraud");
    public static readonly AnomalyType Spike = new("spike");
    public static readonly AnomalyType PatternDeviation = new("pattern_deviation");
}
