using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class DistributionSplitService
{
    public bool ValidateAllocationsSum(DistributionAggregate distribution)
    {
        var allocationsSum = 0m;
        foreach (var allocation in distribution.Allocations)
            allocationsSum += allocation.Amount.Value;

        return allocationsSum == distribution.TotalAmount.Value;
    }
}
