using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Lock;

public sealed record SystemUnlockedEvent(
    LockId LockId,
    SubjectId SubjectId,
    Reason UnlockReason,
    Timestamp UnlockedAt) : DomainEvent;
