namespace Whycespace.Domain.BusinessSystem.Notification.Subscription;

public sealed class CanOptOutSpecification
{
    public bool IsSatisfiedBy(SubscriptionStatus status)
    {
        return status == SubscriptionStatus.OptedIn;
    }
}
