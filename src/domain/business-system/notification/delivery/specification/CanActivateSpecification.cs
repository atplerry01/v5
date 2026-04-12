namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(DeliveryStatus status)
    {
        return status == DeliveryStatus.Draft;
    }
}
