using System.Collections.Concurrent;

namespace Whycespace.Runtime.Observability;

public sealed record MetricEntry
{
    public required string Name { get; init; }
    public required double Value { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
}

public sealed class MetricsCollector
{
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentQueue<MetricEntry> _entries = new();

    public void Increment(string name, Dictionary<string, string>? tags = null)
    {
        _counters.AddOrUpdate(name, 1, (_, v) => v + 1);
        Record(name, 1, tags);
    }

    public void Record(string name, double value, Dictionary<string, string>? tags = null)
    {
        _entries.Enqueue(new MetricEntry
        {
            Name = name,
            Value = value,
            Tags = tags ?? new Dictionary<string, string>()
        });
    }

    public void RecordDuration(string name, TimeSpan duration, Dictionary<string, string>? tags = null)
    {
        Record(name, duration.TotalMilliseconds, tags);
    }

    public long GetCount(string name)
    {
        return _counters.GetValueOrDefault(name, 0);
    }

    public IReadOnlyList<MetricEntry> GetEntries()
    {
        return [.. _entries];
    }

    // Well-known metric names
    public static class Names
    {
        public const string CommandReceived = "runtime.command.received";
        public const string CommandSucceeded = "runtime.command.succeeded";
        public const string CommandFailed = "runtime.command.failed";
        public const string CommandDuration = "runtime.command.duration_ms";
        public const string EngineInvoked = "runtime.engine.invoked";
        public const string EngineSucceeded = "runtime.engine.succeeded";
        public const string EngineFailed = "runtime.engine.failed";
        public const string EngineDuration = "runtime.engine.duration_ms";
        public const string EventEmitted = "runtime.event.emitted";
        public const string EventRouted = "runtime.event.routed";
        public const string WorkflowStarted = "runtime.workflow.started";
        public const string WorkflowCompleted = "runtime.workflow.completed";
        public const string WorkflowFaulted = "runtime.workflow.faulted";

        // API-level metrics (E11.1 hardening)
        public const string ApiRequest = "api.request";
        public const string ApiError = "api.error";
        public const string ApiLatency = "api.latency_ms";

        // Idempotency metrics (E11.1 hardening)
        public const string IdempotencyHit = "idempotency.hit";
        public const string IdempotencyMiss = "idempotency.miss";

        // Command validation metrics (E11.1 hardening)
        public const string ValidationFailed = "runtime.validation.failed";
        public const string DuplicateApprovalBlocked = "runtime.validation.duplicate_approval_blocked";
    }
}
