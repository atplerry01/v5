namespace Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyDecision;

public sealed record PolicyDecisionRecordedEventSchema(
    Guid AggregateId,
    string PolicyDefinitionId,
    string SubjectId,
    string Action,
    string Resource,
    string Outcome,
    DateTimeOffset DecidedAt);
