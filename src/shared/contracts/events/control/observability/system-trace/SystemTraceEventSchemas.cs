namespace Whycespace.Shared.Contracts.Events.Control.Observability.SystemTrace;

public sealed record SystemTraceSpanStartedEventSchema(
    Guid AggregateId,
    string TraceId,
    string OperationName,
    DateTimeOffset StartedAt,
    string? ParentSpanId);

public sealed record SystemTraceSpanCompletedEventSchema(
    Guid AggregateId,
    DateTimeOffset CompletedAt,
    string Status);
