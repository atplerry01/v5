namespace Whycespace.Domain.BusinessSystem.Notification.Subscription;

public sealed class CanResubscribeSpecification
{
    public bool IsSatisfiedBy(SubscriptionStatus status)
    {
        return status == SubscriptionStatus.OptedOut;
    }
}
