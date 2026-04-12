namespace Whycespace.Domain.BusinessSystem.Inventory.Warehouse;

public sealed class CanDeactivateWarehouseSpecification
{
    public bool IsSatisfiedBy(WarehouseStatus status)
    {
        return status == WarehouseStatus.Active;
    }
}
