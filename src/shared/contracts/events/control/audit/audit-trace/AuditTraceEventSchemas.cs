namespace Whycespace.Shared.Contracts.Events.Control.Audit.AuditTrace;

public sealed record AuditTraceOpenedEventSchema(
    Guid AggregateId,
    string CorrelationId,
    DateTimeOffset OpenedAt);

public sealed record AuditTraceEventLinkedEventSchema(
    Guid AggregateId,
    string AuditEventId);

public sealed record AuditTraceClosedEventSchema(
    Guid AggregateId,
    DateTimeOffset ClosedAt);
