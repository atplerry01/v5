namespace Whycespace.Domain.BusinessSystem.Integration.Subscription;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(SubscriptionStatus status)
    {
        return status == SubscriptionStatus.Defined || status == SubscriptionStatus.Deactivated;
    }
}
