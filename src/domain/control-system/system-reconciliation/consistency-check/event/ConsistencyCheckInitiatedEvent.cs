using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ConsistencyCheck;

public sealed record ConsistencyCheckInitiatedEvent(
    ConsistencyCheckId Id,
    string ScopeTarget,
    DateTimeOffset InitiatedAt) : DomainEvent;
