namespace Whycespace.Domain.BusinessSystem.Inventory.Warehouse;

public readonly record struct WarehouseCapacity
{
    public int Value { get; }

    public WarehouseCapacity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("WarehouseCapacity must be positive.", nameof(value));
        Value = value;
    }
}
