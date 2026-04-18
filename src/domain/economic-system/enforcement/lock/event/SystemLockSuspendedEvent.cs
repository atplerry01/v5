using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Lock;

/// <summary>
/// Phase 7 T7.8 — lock temporarily paused for a bounded cause (typically
/// a CompensationFlow against the same subject). The underlying Locked
/// state is preserved; suspension does not release the lock. Expiry is
/// not raised on a Suspended lock — a suspended lock's timer is paused
/// conceptually and must be Resumed first.
/// </summary>
public sealed record SystemLockSuspendedEvent(
    LockId LockId,
    SubjectId SubjectId,
    EnforcementCause SuspensionCause,
    Timestamp SuspendedAt) : DomainEvent;
