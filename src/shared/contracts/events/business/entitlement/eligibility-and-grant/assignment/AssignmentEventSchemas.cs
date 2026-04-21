namespace Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Assignment;

public sealed record AssignmentCreatedEventSchema(
    Guid AggregateId,
    Guid GrantId,
    Guid SubjectId,
    string Scope);

public sealed record AssignmentActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record AssignmentRevokedEventSchema(
    Guid AggregateId,
    DateTimeOffset RevokedAt);
