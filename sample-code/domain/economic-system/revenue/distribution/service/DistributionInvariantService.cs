namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class DistributionInvariantService
{
    public DistributionInvariantResult Validate(
        Money totalRevenue,
        IReadOnlyList<DistributionAllocation> allocations)
    {
        if (allocations == null || allocations.Count == 0)
            return DistributionInvariantResult.Fail("No allocations provided");

        var total = Money.Zero(totalRevenue.Currency);

        for (int i = 0; i < allocations.Count; i++)
        {
            total += allocations[i].Amount;
        }

        if (total != totalRevenue)
            return DistributionInvariantResult.Fail("Distribution mismatch");

        return DistributionInvariantResult.Success();
    }
}

public sealed record DistributionInvariantResult(
    bool IsValid,
    string? Error)
{
    public static DistributionInvariantResult Fail(string error)
        => new(false, error);

    public static DistributionInvariantResult Success()
        => new(true, null);
}
