using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Metric;

public sealed record MetricRecordedEvent(Guid MetricId) : DomainEvent;
