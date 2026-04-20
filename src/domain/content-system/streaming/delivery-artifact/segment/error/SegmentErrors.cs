using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Segment;

public static class SegmentErrors
{
    public static DomainException SegmentArchived()
        => new("Cannot mutate an archived segment.");

    public static DomainException SegmentRetired()
        => new("Cannot publish a retired segment.");

    public static DomainException AlreadyPublished()
        => new("Segment is already published.");

    public static DomainException AlreadyRetired()
        => new("Segment is already retired.");

    public static DomainException AlreadyArchived()
        => new("Segment is already archived.");

    public static DomainInvariantViolationException OrphanedSegment()
        => new("Segment must reference a valid source.");
}
