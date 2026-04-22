using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemHealth;

public sealed record SystemHealthEvaluatedEvent(
    SystemHealthId Id,
    string ComponentName,
    HealthStatus Status,
    DateTimeOffset EvaluatedAt) : DomainEvent;
