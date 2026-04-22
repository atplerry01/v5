using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Observability.SystemTrace;

public sealed record StartSystemTraceCommand(
    Guid SpanId,
    string TraceId,
    string OperationName,
    DateTimeOffset StartedAt,
    string? ParentSpanId = null) : IHasAggregateId
{
    public Guid AggregateId => SpanId;
}

public sealed record CompleteSystemTraceCommand(
    Guid SpanId,
    DateTimeOffset CompletedAt,
    string Status) : IHasAggregateId
{
    public Guid AggregateId => SpanId;
}
