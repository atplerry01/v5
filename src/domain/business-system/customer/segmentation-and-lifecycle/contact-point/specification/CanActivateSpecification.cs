namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ContactPointStatus status) => status == ContactPointStatus.Draft;
}
