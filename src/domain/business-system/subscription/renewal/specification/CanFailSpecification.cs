namespace Whycespace.Domain.BusinessSystem.Subscription.Renewal;

public sealed class CanFailSpecification
{
    public bool IsSatisfiedBy(RenewalStatus status)
    {
        return status == RenewalStatus.Pending;
    }
}
