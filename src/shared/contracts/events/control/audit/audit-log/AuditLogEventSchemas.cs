namespace Whycespace.Shared.Contracts.Events.Control.Audit.AuditLog;

public sealed record AuditEntryRecordedEventSchema(
    Guid AggregateId,
    string ActorId,
    string Action,
    string Resource,
    string Classification,
    DateTimeOffset OccurredAt,
    string? DecisionId);
