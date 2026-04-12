namespace Whycespace.Domain.BusinessSystem.Inventory.Stock;

public readonly record struct StockId
{
    public Guid Value { get; }

    public StockId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StockId value must not be empty.", nameof(value));
        Value = value;
    }
}
