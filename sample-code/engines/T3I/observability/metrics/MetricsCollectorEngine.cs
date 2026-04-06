using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.Observability.Metrics;

/// <summary>
/// T3I engine: produces metric entries as read models.
/// Does NOT write to sinks — returns MetricEntry for runtime to route.
/// </summary>
public sealed class MetricsCollectorEngine
{
    private readonly IClock _clock;

    public MetricsCollectorEngine(IClock clock)
    {
        _clock = clock;
    }

    public MetricEntry RecordExecutionTime(string workflowName, TimeSpan duration)
    {
        return new MetricEntry
        {
            Name = "workflow.execution_time_ms",
            Value = duration.TotalMilliseconds,
            Tags = new Dictionary<string, string> { ["workflow"] = workflowName },
            Timestamp = _clock.UtcNowOffset
        };
    }

    public MetricEntry RecordWorkflowOutcome(string workflowName, bool success)
    {
        return new MetricEntry
        {
            Name = success ? "workflow.success" : "workflow.failure",
            Value = 1,
            Tags = new Dictionary<string, string> { ["workflow"] = workflowName },
            Timestamp = _clock.UtcNowOffset
        };
    }

    public MetricEntry RecordThroughput(string commandType)
    {
        return new MetricEntry
        {
            Name = "command.throughput",
            Value = 1,
            Tags = new Dictionary<string, string> { ["command_type"] = commandType },
            Timestamp = _clock.UtcNowOffset
        };
    }

    public MetricEntry RecordEngineLatency(string engineName, TimeSpan latency)
    {
        return new MetricEntry
        {
            Name = "engine.latency_ms",
            Value = latency.TotalMilliseconds,
            Tags = new Dictionary<string, string> { ["engine"] = engineName },
            Timestamp = _clock.UtcNowOffset
        };
    }

    public MetricEntry RecordCustomMetric(string name, double value, IReadOnlyDictionary<string, string>? tags = null)
    {
        return new MetricEntry
        {
            Name = name,
            Value = value,
            Tags = tags ?? new Dictionary<string, string>(),
            Timestamp = _clock.UtcNowOffset
        };
    }
}
