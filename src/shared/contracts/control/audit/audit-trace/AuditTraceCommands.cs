using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Audit.AuditTrace;

public sealed record OpenAuditTraceCommand(
    Guid TraceId,
    string CorrelationId,
    DateTimeOffset OpenedAt) : IHasAggregateId
{
    public Guid AggregateId => TraceId;
}

public sealed record LinkAuditTraceEventCommand(
    Guid TraceId,
    string AuditEventId) : IHasAggregateId
{
    public Guid AggregateId => TraceId;
}

public sealed record CloseAuditTraceCommand(
    Guid TraceId,
    DateTimeOffset ClosedAt) : IHasAggregateId
{
    public Guid AggregateId => TraceId;
}
