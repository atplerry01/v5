namespace Whycespace.Domain.BusinessSystem.Inventory.Stock;

public readonly record struct Quantity
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value < 0)
            throw new ArgumentException("Quantity must not be negative.", nameof(value));
        Value = value;
    }
}
