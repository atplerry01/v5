using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemMetric;

public sealed record SystemMetricDefinedEvent(SystemMetricId Id, string Name, MetricType Type, string Unit, IReadOnlyList<string> LabelNames) : DomainEvent;
