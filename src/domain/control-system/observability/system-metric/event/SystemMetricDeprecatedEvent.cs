using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemMetric;

public sealed record SystemMetricDeprecatedEvent(SystemMetricId Id) : DomainEvent;
