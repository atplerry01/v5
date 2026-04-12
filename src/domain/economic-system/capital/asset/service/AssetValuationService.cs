using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Capital.Asset;

public sealed class AssetValuationService
{
    public Amount CalculateAppreciation(AssetAggregate asset, Amount newValue)
    {
        var difference = newValue.Value - asset.Value.Value;
        return difference > 0m ? new Amount(difference) : new Amount(0m);
    }
}
