namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(DeliveryStatus status)
    {
        return status == DeliveryStatus.Active;
    }
}
