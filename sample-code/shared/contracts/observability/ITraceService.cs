namespace Whycespace.Shared.Contracts.Observability;

public interface ITraceService
{
    TraceContext StartTrace(string correlationId, string commandType);
    void RecordWorkflowStep(TraceContext context, string workflowName, string stepName);
    void RecordEngineInvocation(TraceContext context, string engineName, TimeSpan duration, bool success);
    void RecordEvent(TraceContext context, string eventType);
    void CompleteTrace(TraceContext context, bool success);
}

public sealed record TraceContext
{
    public required string TraceId { get; init; }
    public required string CorrelationId { get; init; }
    public required string CommandType { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
}

public sealed record TraceEntry
{
    public required string TraceId { get; init; }
    public required string CorrelationId { get; init; }
    public required string Phase { get; init; }
    public required string Component { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public long? DurationMs { get; init; }
    public bool? Success { get; init; }
}

public interface ITraceStore
{
    void Record(TraceEntry entry);
    IReadOnlyList<TraceEntry> GetByTraceId(string traceId);
    IReadOnlyList<TraceEntry> GetByCorrelationId(string correlationId);
}
