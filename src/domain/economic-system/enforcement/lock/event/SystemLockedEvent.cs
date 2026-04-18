using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Lock;

/// <summary>
/// System-locked event. Phase 7 T7.6 added <see cref="Cause"/> and
/// <see cref="ExpiresAt"/> as init-only fields so V2 streams carry the
/// full lifecycle context while V1 streams continue to deserialize
/// (<c>Cause = null</c>, <c>ExpiresAt = null</c>). On V1 replay the
/// aggregate's Apply handler synthesizes a Legacy/Manual cause; an
/// absent <c>ExpiresAt</c> means "no natural expiry" — the lock runs
/// until Unlocked.
/// </summary>
public sealed record SystemLockedEvent(
    LockId LockId,
    SubjectId SubjectId,
    LockScope Scope,
    Reason Reason,
    Timestamp LockedAt) : DomainEvent
{
    public EnforcementCause? Cause { get; init; }
    public Timestamp? ExpiresAt { get; init; }
}
