namespace Whycespace.Domain.BusinessSystem.Inventory.Sku;

public sealed class CanDiscontinueSkuSpecification
{
    public bool IsSatisfiedBy(SkuStatus status)
    {
        return status == SkuStatus.Active;
    }
}
