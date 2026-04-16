using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Lock;

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
}
