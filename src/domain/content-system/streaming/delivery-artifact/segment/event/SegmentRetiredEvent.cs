using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Segment;

public sealed record SegmentRetiredEvent(
    SegmentId SegmentId,
    Timestamp RetiredAt) : DomainEvent;
