namespace Whycespace.Domain.BusinessSystem.Inventory.Warehouse;

public readonly record struct WarehouseId
{
    public Guid Value { get; }

    public WarehouseId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WarehouseId value must not be empty.", nameof(value));
        Value = value;
    }
}