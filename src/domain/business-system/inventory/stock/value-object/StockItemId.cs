namespace Whycespace.Domain.BusinessSystem.Inventory.Stock;

public readonly record struct StockItemId
{
    public Guid Value { get; }

    public StockItemId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StockItemId value must not be empty.", nameof(value));
        Value = value;
    }
}
