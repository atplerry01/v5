namespace Whycespace.Domain.BusinessSystem.Subscription.Usage;

public sealed class CanAggregateSpecification
{
    public bool IsSatisfiedBy(UsageStatus status)
    {
        return status == UsageStatus.Recorded;
    }
}
