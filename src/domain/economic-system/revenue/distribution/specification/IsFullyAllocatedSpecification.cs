using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class IsFullyAllocatedSpecification : Specification<DistributionAggregate>
{
    public override bool IsSatisfiedBy(DistributionAggregate distribution)
    {
        if (distribution.Allocations.Count == 0) return false;

        var allocationsSum = 0m;
        foreach (var allocation in distribution.Allocations)
            allocationsSum += allocation.Amount.Value;

        return allocationsSum == distribution.TotalAmount.Value;
    }
}
