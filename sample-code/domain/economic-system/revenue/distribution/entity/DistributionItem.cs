namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class DistributionItem
{
    public Guid Id { get; }

    public DistributionItem(Guid id)
    {
        Id = id;
    }
}
