namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(SegmentStatus status) => status != SegmentStatus.Archived;
}
