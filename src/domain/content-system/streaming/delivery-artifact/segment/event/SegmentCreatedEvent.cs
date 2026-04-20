using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Segment;

public sealed record SegmentCreatedEvent(
    SegmentId SegmentId,
    SegmentSourceRef SourceRef,
    SegmentSequenceNumber Sequence,
    SegmentWindow Window,
    Timestamp CreatedAt) : DomainEvent;
