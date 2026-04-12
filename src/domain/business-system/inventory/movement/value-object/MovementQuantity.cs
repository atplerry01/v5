namespace Whycespace.Domain.BusinessSystem.Inventory.Movement;

public readonly record struct MovementQuantity
{
    public int Value { get; }

    public MovementQuantity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Movement quantity must be greater than zero.", nameof(value));
        Value = value;
    }
}
