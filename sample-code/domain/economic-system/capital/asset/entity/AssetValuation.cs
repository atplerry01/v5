namespace Whycespace.Domain.EconomicSystem.Capital.Asset;

public sealed class AssetValuation
{
    public Guid Id { get; }

    public AssetValuation(Guid id)
    {
        Id = id;
    }
}
