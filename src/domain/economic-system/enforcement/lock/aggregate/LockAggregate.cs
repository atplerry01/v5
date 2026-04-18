using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Lock;

/// <summary>
/// Lock aggregate.
///
/// Phase 7 T7.6/T7.8/T7.9 — every Lock carries an
/// <see cref="EnforcementCause"/> coupling it to its triggering
/// aggregate, plus an optional <see cref="ExpiresAt"/> for natural
/// expiry. The state machine is extended with a reversible
/// <see cref="LockStatus.Suspended"/> pause (so compensation flows on
/// the same subject aren't blocked by their own enforcement remnants)
/// and a distinct terminal <see cref="LockStatus.Expired"/> state so
/// natural expiry is disambiguated from explicit unlock.
///
/// State transitions (all other pairs are rejected):
///   Lock       →  Locked
///   Suspend    :  Locked    → Suspended
///   Resume     :  Suspended → Locked
///   Unlock     :  Locked | Suspended → Unlocked  (terminal — explicit release)
///   Expire     :  Locked             → Expired   (terminal — natural expiry)
///
/// Suspended locks cannot expire — a paused timer doesn't count down.
/// Resume first, then Expire or Unlock.
///
/// Backward compatibility: V1 streams (pre-T7.6) carry
/// <see cref="SystemLockedEvent"/> instances with <c>Cause = null</c>
/// and <c>ExpiresAt = null</c>. The Apply handler synthesizes a
/// <see cref="EnforcementCause.Legacy"/> cause; absent ExpiresAt
/// signals "no natural expiry — runs until Unlocked".
/// </summary>
public sealed class LockAggregate : AggregateRoot
{
    public LockId LockId { get; private set; }
    public SubjectId SubjectId { get; private set; }
    public LockScope Scope { get; private set; }
    public Reason Reason { get; private set; }
    public LockStatus Status { get; private set; }
    public Timestamp LockedAt { get; private set; }

    public EnforcementCause? Cause { get; private set; }
    public Timestamp? ExpiresAt { get; private set; }

    private LockAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static LockAggregate Lock(
        LockId lockId,
        SubjectId subjectId,
        LockScope scope,
        Reason reason,
        EnforcementCause cause,
        Timestamp lockedAt,
        Timestamp? expiresAt = null)
    {
        if (subjectId.Value == Guid.Empty) throw LockErrors.MissingSubjectReference();
        if (string.IsNullOrWhiteSpace(reason.Value)) throw LockErrors.MissingReason();
        if (cause is null) throw LockErrors.CauseRequired();
        if (expiresAt is not null && expiresAt.Value.Value <= lockedAt.Value)
            throw LockErrors.ExpiresAtMustBeAfterLockedAt();

        var aggregate = new LockAggregate();
        aggregate.RaiseDomainEvent(new SystemLockedEvent(
            lockId, subjectId, scope, reason, lockedAt)
        {
            Cause = cause,
            ExpiresAt = expiresAt
        });
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    /// <summary>
    /// Phase 7 T7.8 — temporarily nullify enforcement for a bounded
    /// cause (typically a compensation flow against the same subject).
    /// The original Lock-time <see cref="Cause"/>, <see cref="Scope"/>,
    /// <see cref="Reason"/>, and <see cref="ExpiresAt"/> are preserved;
    /// <see cref="Resume"/> restores the pre-suspension state exactly.
    /// </summary>
    public void Suspend(EnforcementCause suspensionCause, Timestamp suspendedAt)
    {
        if (suspensionCause is null) throw LockErrors.CauseRequired();
        if (Status != LockStatus.Locked)
            throw LockErrors.CannotSuspendNonLocked(Status);

        RaiseDomainEvent(new SystemLockSuspendedEvent(
            LockId, SubjectId, suspensionCause, suspendedAt));
    }

    public void Resume(Timestamp resumedAt)
    {
        if (Status != LockStatus.Suspended)
            throw LockErrors.CannotResumeNonSuspended(Status);

        RaiseDomainEvent(new SystemLockResumedEvent(LockId, SubjectId, resumedAt));
    }

    public void Unlock(Reason unlockReason, Timestamp unlockedAt)
    {
        if (Status == LockStatus.Unlocked) throw LockErrors.AlreadyUnlocked();
        if (Status == LockStatus.Expired)
            throw LockErrors.CannotUnlockTerminalLock(Status);
        if (string.IsNullOrWhiteSpace(unlockReason.Value)) throw LockErrors.MissingReason();

        RaiseDomainEvent(new SystemUnlockedEvent(LockId, SubjectId, unlockReason, unlockedAt));
    }

    /// <summary>
    /// Phase 7 T7.9 — natural expiry. Only valid from Locked; a suspended
    /// lock's timer is paused and must be Resumed before it can expire.
    /// Scheduling the ExpireSystemLock dispatch itself is out of this
    /// batch's scope — the aggregate simply honours the transition when
    /// the command arrives.
    /// </summary>
    public void Expire(Timestamp expiredAt)
    {
        if (Status != LockStatus.Locked)
            throw LockErrors.CannotExpireNonLocked(Status);

        RaiseDomainEvent(new SystemLockExpiredEvent(LockId, SubjectId, expiredAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SystemLockedEvent e:
                LockId = e.LockId;
                SubjectId = e.SubjectId;
                Scope = e.Scope;
                Reason = e.Reason;
                Status = LockStatus.Locked;
                LockedAt = e.LockedAt;
                Cause = e.Cause ?? EnforcementCause.Legacy(e.SubjectId.Value);
                ExpiresAt = e.ExpiresAt;
                break;

            case SystemLockSuspendedEvent:
                Status = LockStatus.Suspended;
                break;

            case SystemLockResumedEvent:
                Status = LockStatus.Locked;
                break;

            case SystemUnlockedEvent:
                Status = LockStatus.Unlocked;
                break;

            case SystemLockExpiredEvent:
                Status = LockStatus.Expired;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (LockId.Value == Guid.Empty) throw LockErrors.EmptyLockId();
        if (SubjectId.Value == Guid.Empty) throw LockErrors.OrphanLock();

        // Phase 7 T7.6 — cause-coupling. Active states require a cause.
        if ((Status == LockStatus.Locked || Status == LockStatus.Suspended)
            && Cause is null)
            throw LockErrors.CauseMissingOnActiveLock();
    }
}
