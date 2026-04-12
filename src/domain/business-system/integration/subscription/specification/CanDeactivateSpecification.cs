namespace Whycespace.Domain.BusinessSystem.Integration.Subscription;

public sealed class CanDeactivateSpecification
{
    public bool IsSatisfiedBy(SubscriptionStatus status)
    {
        return status == SubscriptionStatus.Active;
    }
}
