namespace Whycespace.Domain.BusinessSystem.Inventory.Writeoff;

public readonly record struct WriteoffQuantity
{
    public int Value { get; }

    public WriteoffQuantity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("WriteoffQuantity must be positive.", nameof(value));
        Value = value;
    }
}
