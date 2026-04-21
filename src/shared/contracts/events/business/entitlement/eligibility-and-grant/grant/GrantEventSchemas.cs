namespace Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Grant;

public sealed record GrantCreatedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    Guid TargetId,
    string Scope,
    DateTimeOffset? ExpiresAt);

public sealed record GrantActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record GrantRevokedEventSchema(
    Guid AggregateId,
    DateTimeOffset RevokedAt);

public sealed record GrantExpiredEventSchema(
    Guid AggregateId,
    DateTimeOffset ExpiredAt);
