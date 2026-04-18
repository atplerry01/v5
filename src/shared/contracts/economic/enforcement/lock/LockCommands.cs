using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Enforcement.Lock;

/// <summary>
/// Phase 7 T7.6 — command-side DTO for <c>EnforcementCause</c>. Parallel
/// to the Restriction sub-domain's DTO per existing per-context
/// duplication convention.
/// </summary>
public sealed record EnforcementCauseDto(
    string Kind,
    Guid CauseReferenceId,
    string Detail);

public sealed record LockSystemCommand(
    Guid LockId,
    Guid SubjectId,
    string Scope,
    string Reason,
    DateTimeOffset LockedAt) : IHasAggregateId
{
    public Guid AggregateId => LockId;

    // Phase 7 T7.6 — optional on the wire; handler synthesizes a Manual
    // cause when absent so the aggregate's non-null-cause invariant holds.
    public EnforcementCauseDto? Cause { get; init; }

    // Phase 7 T7.9 — optional natural-expiry timestamp. Null means the
    // lock runs until explicitly Unlocked.
    public DateTimeOffset? ExpiresAt { get; init; }
}

public sealed record UnlockSystemCommand(
    Guid LockId,
    string UnlockReason,
    DateTimeOffset UnlockedAt) : IHasAggregateId
{
    public Guid AggregateId => LockId;
}

// ── Phase 7 T7.8 — suspend/resume ─────────────────────────────────

public sealed record SuspendSystemLockCommand(
    Guid LockId,
    EnforcementCauseDto SuspensionCause,
    DateTimeOffset SuspendedAt) : IHasAggregateId
{
    public Guid AggregateId => LockId;
}

public sealed record ResumeSystemLockCommand(
    Guid LockId,
    DateTimeOffset ResumedAt) : IHasAggregateId
{
    public Guid AggregateId => LockId;
}

// ── Phase 7 T7.9 — natural expiry ────────────────────────────────

public sealed record ExpireSystemLockCommand(
    Guid LockId,
    DateTimeOffset ExpiredAt) : IHasAggregateId
{
    public Guid AggregateId => LockId;
}
