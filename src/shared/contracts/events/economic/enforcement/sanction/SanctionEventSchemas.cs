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

/// <summary>
/// V1 wire shape: the positional constructor below. V2 (Phase 7 B5 /
/// T7.10) adds <see cref="Enforcement"/> as an init-only nullable
/// property so V1 messages deserialize with <c>Enforcement = null</c>.
/// New messages carry the authoritative sanction → enforcement
/// coupling: the deterministic <c>RestrictionId</c> or <c>LockId</c>
/// derived from the sanction id at Activate-time.
/// </summary>
public sealed record SanctionActivatedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    DateTimeOffset ActivatedAt)
{
    public SanctionEnforcementRefDto? Enforcement { get; init; }
}

public sealed record SanctionExpiredEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    DateTimeOffset ExpiredAt);

public sealed record SanctionRevokedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string RevocationReason,
    DateTimeOffset RevokedAt);

/// <summary>
/// Phase 7 B5 / T7.10 — wire-safe DTO for the domain
/// <c>EnforcementRef</c> value object. Kind is serialised as its
/// <c>SanctionType</c> enum name: <c>Restriction | Lock</c>.
/// EnforcementId is the deterministic RestrictionId / LockId the
/// sanction acts through.
/// </summary>
public sealed record SanctionEnforcementRefDto(
    string Kind,
    Guid EnforcementId);
