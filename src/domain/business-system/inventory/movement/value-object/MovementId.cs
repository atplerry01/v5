namespace Whycespace.Domain.BusinessSystem.Inventory.Movement;

public readonly record struct MovementId
{
    public Guid Value { get; }

    public MovementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MovementId value must not be empty.", nameof(value));
        Value = value;
    }
}
