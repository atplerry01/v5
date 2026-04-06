namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class DistributionEligibilitySpec
{
    public bool IsSatisfiedBy(DistributionAggregate distribution)
    {
        return distribution.TotalAmount is not null
            && !distribution.TotalAmount.IsZero
            && !distribution.TotalAmount.IsNegative
            && distribution.RevenueId != Guid.Empty;
    }
}
