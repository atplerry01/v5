using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ConsistencyCheck;

public sealed record ConsistencyCheckCompletedEvent(
    ConsistencyCheckId Id,
    bool HasDiscrepancies,
    DateTimeOffset CompletedAt) : DomainEvent;
