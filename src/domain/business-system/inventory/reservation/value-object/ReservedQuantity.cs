namespace Whycespace.Domain.BusinessSystem.Inventory.Reservation;

public readonly record struct ReservedQuantity
{
    public int Value { get; }

    public ReservedQuantity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Reserved quantity must be greater than zero.", nameof(value));
        Value = value;
    }
}
