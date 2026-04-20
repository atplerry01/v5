using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Segment;

public sealed record SegmentArchivedEvent(
    SegmentId SegmentId,
    Timestamp ArchivedAt) : DomainEvent;
