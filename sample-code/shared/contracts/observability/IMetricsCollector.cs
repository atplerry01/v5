namespace Whycespace.Shared.Contracts.Observability;

public interface IMetricsCollector
{
    void RecordExecutionTime(string name, TimeSpan duration);
    void RecordThroughput(string name);
    void RecordWorkflowOutcome(string name, bool success);
    void RecordEngineLatency(string engineName, TimeSpan latency);
    void RecordCustomMetric(string name, double value, IReadOnlyDictionary<string, string>? tags = null);
}

public sealed record MetricEntry
{
    public required string Name { get; init; }
    public required double Value { get; init; }
    public required IReadOnlyDictionary<string, string> Tags { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}

public interface IMetricsSink
{
    void Record(MetricEntry entry);
}
