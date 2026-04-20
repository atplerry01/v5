using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Segment;

public sealed class CanPublishSegmentSpecification : Specification<SegmentAggregate>
{
    public override bool IsSatisfiedBy(SegmentAggregate entity)
        => entity.Status == SegmentStatus.Created;
}
