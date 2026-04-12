namespace Whycespace.Domain.BusinessSystem.Inventory.Stock;

public sealed class CanDepleteSpecification
{
    public bool IsSatisfiedBy(StockStatus status)
    {
        return status == StockStatus.Tracked;
    }
}
