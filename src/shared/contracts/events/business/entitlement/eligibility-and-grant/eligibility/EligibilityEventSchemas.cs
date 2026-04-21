namespace Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Eligibility;

public sealed record EligibilityCreatedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    Guid TargetId,
    string Scope);

public sealed record EligibilityEvaluatedEligibleEventSchema(
    Guid AggregateId,
    DateTimeOffset EvaluatedAt);

public sealed record EligibilityEvaluatedIneligibleEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset EvaluatedAt);
