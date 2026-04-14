using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class IsFullyAllocatedSpecification : Specification<DistributionAggregate>
{
    public override bool IsSatisfiedBy(DistributionAggregate distribution)
    {
        if (distribution.Shares.Count == 0) return false;

        decimal percentageSum = 0m;
        foreach (var share in distribution.Shares)
            percentageSum += share.Percentage;

        return percentageSum == 100m;
    }
}
