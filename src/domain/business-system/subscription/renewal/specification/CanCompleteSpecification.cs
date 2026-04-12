namespace Whycespace.Domain.BusinessSystem.Subscription.Renewal;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(RenewalStatus status)
    {
        return status == RenewalStatus.Pending;
    }
}
