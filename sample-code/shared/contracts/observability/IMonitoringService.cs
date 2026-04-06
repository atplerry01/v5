namespace Whycespace.Shared.Contracts.Observability;

public interface IMonitoringService
{
    void EvaluateMetric(MetricEntry metric, IReadOnlyList<AlertThreshold> thresholds);
}

public interface IMonitoringAlertSink
{
    void Emit(MonitoringAlert alert);
}

public sealed record MonitoringAlert
{
    public required string AlertId { get; init; }
    public required string MetricName { get; init; }
    public required double ActualValue { get; init; }
    public required double ThresholdValue { get; init; }
    public required AlertSeverity Severity { get; init; }
    public required string Message { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}

public sealed record AlertThreshold
{
    public required string MetricName { get; init; }
    public required double Value { get; init; }
    public required ThresholdDirection Direction { get; init; }
    public required AlertSeverity Severity { get; init; }
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public enum ThresholdDirection
{
    Above,
    Below
}
