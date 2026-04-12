namespace Whycespace.Domain.BusinessSystem.Inventory.Stock;

public sealed class CanTrackSpecification
{
    public bool IsSatisfiedBy(StockStatus status)
    {
        return status == StockStatus.Initialized;
    }
}
