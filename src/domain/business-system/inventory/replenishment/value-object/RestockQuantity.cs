namespace Whycespace.Domain.BusinessSystem.Inventory.Replenishment;

public readonly record struct RestockQuantity
{
    public int Value { get; }

    public RestockQuantity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("RestockQuantity must be positive.", nameof(value));
        Value = value;
    }
}
