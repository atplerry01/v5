using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public sealed record MetricsArchivedEvent(
    MetricsId MetricsId,
    Timestamp ArchivedAt) : DomainEvent;
