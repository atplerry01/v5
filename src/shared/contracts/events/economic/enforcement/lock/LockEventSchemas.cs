namespace Whycespace.Shared.Contracts.Events.Economic.Enforcement.Lock;

/// <summary>
/// V1 wire shape: the positional constructor below. V2 (Phase 7 B4)
/// adds <see cref="Cause"/> and <see cref="ExpiresAt"/> as init-only
/// nullable properties so V1 messages deserialize with nulls; new
/// messages carry the explicit cause coupling and the optional natural-
/// expiry timestamp.
/// </summary>
public sealed record SystemLockedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string Scope,
    string Reason,
    DateTimeOffset LockedAt)
{
    public LockEnforcementCauseDto? Cause { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}

public sealed record SystemUnlockedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string UnlockReason,
    DateTimeOffset UnlockedAt);

/// <summary>
/// Phase 7 B4 / T7.8 — lock temporarily paused. The underlying Locked
/// state (cause, scope, reason, expiry) is preserved on the aggregate;
/// the SuspensionCause carried here identifies the triggering flow.
/// </summary>
public sealed record SystemLockSuspendedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    LockEnforcementCauseDto SuspensionCause,
    DateTimeOffset SuspendedAt);

/// <summary>
/// Phase 7 B4 / T7.8 — lock resumed from suspension. The aggregate
/// restores its Lock-time state exactly; no new fields are introduced.
/// </summary>
public sealed record SystemLockResumedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    DateTimeOffset ResumedAt);

/// <summary>
/// Phase 7 B4 / T7.9 — natural terminal expiry, distinct from explicit
/// release (<see cref="SystemUnlockedEventSchema"/>). Expiry is never
/// raised from a Suspended lock.
/// </summary>
public sealed record SystemLockExpiredEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    DateTimeOffset ExpiredAt);

/// <summary>
/// Phase 7 B4 / T7.6 — wire-safe DTO for the domain
/// <c>EnforcementCause</c> value object (lock sub-domain). Duplicated
/// per the project's per-context VO convention; Kind values match
/// <c>EnforcementCauseKind</c>:
/// <c>Sanction | PayoutFailure | CompensationFlow | ComplianceViolation | Manual</c>.
/// </summary>
public sealed record LockEnforcementCauseDto(
    string Kind,
    Guid CauseReferenceId,
    string Detail);
