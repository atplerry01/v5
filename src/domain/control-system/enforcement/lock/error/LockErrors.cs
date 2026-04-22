using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Lock;

public static class LockErrors
{
    public static DomainException MissingSubjectReference() =>
        new("Lock must reference a subject.");

    public static DomainException MissingReason() =>
        new("Lock must include a reason.");

    public static DomainException AlreadyUnlocked() =>
        new("Lock has already been released.");

    public static DomainException CannotLockTwice() =>
        new("Subject is already locked — cannot re-lock without unlocking first.");

    public static DomainInvariantViolationException EmptyLockId() =>
        new("Invariant violated: LockId cannot be empty.");

    public static DomainInvariantViolationException OrphanLock() =>
        new("Invariant violated: lock must reference a subject.");

    // ── Phase 7 T7.6/T7.8/T7.9 — cause-coupling + suspend/resume/expire ──

    public static DomainException CauseRequired() =>
        new("Lock requires a non-null EnforcementCause (Phase 7 T7.6).");

    public static DomainException CannotSuspendNonLocked(LockStatus current) =>
        new($"Lock can only be suspended from Locked state (was {current}).");

    public static DomainException CannotResumeNonSuspended(LockStatus current) =>
        new($"Lock can only be resumed from Suspended state (was {current}).");

    public static DomainException CannotExpireNonLocked(LockStatus current) =>
        new($"Lock can only be expired from Locked state (was {current}). " +
            "A suspended lock must be resumed before it can expire.");

    public static DomainException CannotUnlockTerminalLock(LockStatus current) =>
        new($"Cannot unlock a lock in terminal state {current}. Unlock is only valid from Locked or Suspended.");

    public static DomainException ExpiresAtMustBeAfterLockedAt() =>
        new("ExpiresAt must be strictly after LockedAt when provided.");

    public static DomainInvariantViolationException CauseMissingOnActiveLock() =>
        new("Invariant violated: a Locked or Suspended lock must carry a non-null Cause.");
}
