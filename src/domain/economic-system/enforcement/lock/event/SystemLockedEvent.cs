using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Lock;

public sealed record SystemLockedEvent(
    LockId LockId,
    SubjectId SubjectId,
    LockScope Scope,
    Reason Reason,
    Timestamp LockedAt) : DomainEvent;
