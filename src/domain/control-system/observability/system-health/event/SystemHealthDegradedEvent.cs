using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemHealth;

public sealed record SystemHealthDegradedEvent(
    SystemHealthId Id,
    HealthStatus NewStatus,
    string Reason,
    DateTimeOffset OccurredAt) : DomainEvent;
