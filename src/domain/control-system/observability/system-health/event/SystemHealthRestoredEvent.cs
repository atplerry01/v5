using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemHealth;

public sealed record SystemHealthRestoredEvent(
    SystemHealthId Id,
    DateTimeOffset RestoredAt) : DomainEvent;
