using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public sealed record MetricsUpdatedEvent(
    MetricsId MetricsId,
    MetricsSnapshot PreviousSnapshot,
    MetricsSnapshot NewSnapshot,
    Timestamp UpdatedAt) : DomainEvent;
