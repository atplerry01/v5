namespace Whycespace.Domain.BusinessSystem.Integration.Subscription;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(SubscriptionStatus status)
    {
        return status == SubscriptionStatus.Active;
    }
}
