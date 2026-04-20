using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public sealed record MetricsFinalizedEvent(
    MetricsId MetricsId,
    Timestamp FinalizedAt) : DomainEvent;
