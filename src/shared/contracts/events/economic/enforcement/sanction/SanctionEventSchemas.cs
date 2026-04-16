namespace Whycespace.Shared.Contracts.Events.Economic.Enforcement.Sanction;

public sealed record SanctionIssuedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string Type,
    string Scope,
    string Reason,
    DateTimeOffset EffectiveAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset IssuedAt);

public sealed record SanctionActivatedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    DateTimeOffset ActivatedAt);

public sealed record SanctionExpiredEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    DateTimeOffset ExpiredAt);

public sealed record SanctionRevokedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string RevocationReason,
    DateTimeOffset RevokedAt);
