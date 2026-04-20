namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(SegmentStatus status) => status == SegmentStatus.Draft;
}
