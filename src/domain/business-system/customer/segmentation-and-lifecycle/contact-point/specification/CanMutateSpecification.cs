namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ContactPointStatus status) => status != ContactPointStatus.Archived;
}
