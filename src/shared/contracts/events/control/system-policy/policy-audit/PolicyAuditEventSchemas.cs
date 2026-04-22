namespace Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyAudit;

public sealed record PolicyAuditEntryRecordedEventSchema(
    Guid AggregateId,
    string PolicyId,
    string ActorId,
    string Action,
    string Category,
    string DecisionHash,
    string CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record PolicyAuditEntryReviewedEventSchema(
    Guid AggregateId,
    string ReviewerId,
    string Reason);
