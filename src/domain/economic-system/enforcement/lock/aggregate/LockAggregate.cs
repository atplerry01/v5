using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Lock;

public sealed class LockAggregate : AggregateRoot
{
    public LockId LockId { get; private set; }
    public SubjectId SubjectId { get; private set; }
    public LockScope Scope { get; private set; }
    public Reason Reason { get; private set; }
    public LockStatus Status { get; private set; }
    public Timestamp LockedAt { get; private set; }

    private LockAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static LockAggregate Lock(
        LockId lockId,
        SubjectId subjectId,
        LockScope scope,
        Reason reason,
        Timestamp lockedAt)
    {
        if (subjectId.Value == Guid.Empty) throw LockErrors.MissingSubjectReference();
        if (string.IsNullOrWhiteSpace(reason.Value)) throw LockErrors.MissingReason();

        var aggregate = new LockAggregate();
        aggregate.RaiseDomainEvent(new SystemLockedEvent(
            lockId, subjectId, scope, reason, lockedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Unlock(Reason unlockReason, Timestamp unlockedAt)
    {
        if (Status == LockStatus.Unlocked) throw LockErrors.AlreadyUnlocked();
        if (string.IsNullOrWhiteSpace(unlockReason.Value)) throw LockErrors.MissingReason();

        RaiseDomainEvent(new SystemUnlockedEvent(LockId, SubjectId, unlockReason, unlockedAt));
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
                break;

            case SystemUnlockedEvent:
                Status = LockStatus.Unlocked;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (LockId.Value == Guid.Empty) throw LockErrors.EmptyLockId();
        if (SubjectId.Value == Guid.Empty) throw LockErrors.OrphanLock();
    }
}
