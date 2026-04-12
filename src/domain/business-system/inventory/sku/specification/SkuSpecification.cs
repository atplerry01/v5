namespace Whycespace.Domain.BusinessSystem.Inventory.Sku;

public sealed class SkuSpecification
{
    public bool IsSatisfiedBy(SkuAggregate sku)
    {
        return sku.Id != default
            && !string.IsNullOrWhiteSpace(sku.Code.Value)
            && Enum.IsDefined(sku.Status);
    }
}
