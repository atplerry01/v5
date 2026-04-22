using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Lock;

/// <summary>
/// Phase 7 T7.9 — natural expiry of a Locked lock whose
/// <see cref="SystemLockedEvent.ExpiresAt"/> has been reached. Terminal
/// and irreversible (symmetric with Unlocked). Scheduling the expiry
/// trigger itself is out of scope for this batch; this event records
/// the state transition once an ExpireSystemLockCommand is dispatched.
/// </summary>
public sealed record SystemLockExpiredEvent(
    LockId LockId,
    SubjectId SubjectId,
    Timestamp ExpiredAt) : DomainEvent;
