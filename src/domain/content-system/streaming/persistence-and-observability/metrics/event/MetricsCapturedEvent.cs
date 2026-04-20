using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public sealed record MetricsCapturedEvent(
    MetricsId MetricsId,
    StreamRef StreamRef,
    RecordingRef? RecordingRef,
    MetricsWindow Window,
    MetricsSnapshot Snapshot,
    Timestamp CapturedAt) : DomainEvent;
