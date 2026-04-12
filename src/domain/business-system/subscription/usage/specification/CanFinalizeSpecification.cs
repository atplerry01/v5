namespace Whycespace.Domain.BusinessSystem.Subscription.Usage;

public sealed class CanFinalizeSpecification
{
    public bool IsSatisfiedBy(UsageStatus status)
    {
        return status == UsageStatus.Aggregated;
    }
}
