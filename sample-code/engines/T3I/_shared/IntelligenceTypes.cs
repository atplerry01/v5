namespace Whycespace.Engines.T3I;

/// <summary>
/// Standardized severity classification for all intelligence engines.
/// </summary>
public enum AlertSeverity
{
    Info = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Confidence score with semantic meaning.
/// </summary>
public sealed record ConfidenceScore(decimal Value)
{
    public static ConfidenceScore High => new(0.9m);
    public static ConfidenceScore Medium => new(0.75m);
    public static ConfidenceScore Low => new(0.6m);
    public static ConfidenceScore Uncertain => new(0.4m);

    public bool IsReliable => Value >= 0.7m;
    public bool IsActionable => Value >= 0.8m;

    public static ConfidenceScore FromDeviation(decimal deviationPercent) => deviationPercent switch
    {
        >= 50m => High,
        >= 25m => new(0.85m),
        >= 10m => Medium,
        _ => Low
    };
}

/// <summary>
/// Alert threshold configuration for intelligence engines.
/// </summary>
public sealed record AlertThreshold(
    string MetricName,
    decimal WarningLevel,
    decimal CriticalLevel,
    string Unit)
{
    public AlertSeverity Evaluate(decimal currentValue) => currentValue switch
    {
        var v when v >= CriticalLevel => AlertSeverity.Critical,
        var v when v >= WarningLevel => AlertSeverity.High,
        var v when v >= WarningLevel * 0.7m => AlertSeverity.Medium,
        var v when v >= WarningLevel * 0.4m => AlertSeverity.Low,
        _ => AlertSeverity.Info
    };
}

/// <summary>
/// Standardized intelligence alert produced by any T3I engine.
/// </summary>
public sealed record IntelligenceAlert(
    string AlertId,
    string EngineSource,
    AlertSeverity Severity,
    ConfidenceScore Confidence,
    string Category,
    string Description,
    IReadOnlyDictionary<string, string> Metadata);

/// <summary>
/// Registry of alert thresholds used across intelligence engines.
/// Stateless, deterministic — no external config.
/// </summary>
public static class AlertThresholdRegistry
{
    public static AlertThreshold RevenueDeviation => new("revenue_deviation", 25m, 50m, "percent");
    public static AlertThreshold PolicyViolationRate => new("policy_violation_rate", 5m, 15m, "count_per_hour");
    public static AlertThreshold WorkflowFailureRate => new("workflow_failure_rate", 0.1m, 0.3m, "ratio");
    public static AlertThreshold CapitalUtilization => new("capital_utilization", 0.8m, 0.95m, "ratio");
    public static AlertThreshold LoginFrequency => new("login_frequency", 15m, 30m, "count_per_day");
    public static AlertThreshold PayoutFrequency => new("payout_frequency", 10m, 25m, "count_per_day");
    public static AlertThreshold ChargeReversalRate => new("charge_reversal_rate", 0.1m, 0.3m, "ratio");
}
