using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Segment;

public sealed record SegmentPublishedEvent(
    SegmentId SegmentId,
    Timestamp PublishedAt) : DomainEvent;
