namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public sealed class CanResumeSpecification
{
    public bool IsSatisfiedBy(DeliveryStatus status)
    {
        return status == DeliveryStatus.Suspended;
    }
}
