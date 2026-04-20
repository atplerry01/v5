namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed class CanCloseSpecification
{
    public bool IsSatisfiedBy(LifecycleStatus status) => status == LifecycleStatus.Tracking;
}
