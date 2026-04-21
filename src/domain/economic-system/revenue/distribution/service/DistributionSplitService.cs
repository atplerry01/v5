namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class DistributionSplitService
{
    public bool ValidateAllocationsSum(DistributionAggregate distribution)
    {
        decimal sharesSum = 0m;
        foreach (var share in distribution.Shares)
            sharesSum += share.Amount;

        return sharesSum == distribution.TotalAmount.Value;
    }
}
