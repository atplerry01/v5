namespace Whycespace.Domain.BusinessSystem.Inventory.Warehouse;

public sealed class WarehouseSpecification
{
    public bool IsSatisfiedBy(WarehouseAggregate warehouse)
    {
        return warehouse.Id != default
            && warehouse.Capacity.Value > 0
            && Enum.IsDefined(warehouse.Status);
    }
}
