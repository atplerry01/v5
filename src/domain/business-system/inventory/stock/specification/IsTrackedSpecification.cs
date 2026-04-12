namespace Whycespace.Domain.BusinessSystem.Inventory.Stock;

public sealed class IsTrackedSpecification
{
    public bool IsSatisfiedBy(StockStatus status)
    {
        return status == StockStatus.Tracked;
    }
}
