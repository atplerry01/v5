namespace Whycespace.Shared.Contracts.Events.Control.Audit.AuditEvent;

public sealed record AuditEventCapturedEventSchema(
    Guid AggregateId,
    string ActorId,
    string Action,
    string Kind,
    string CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record AuditEventSealedEventSchema(
    Guid AggregateId,
    string IntegrityHash);
