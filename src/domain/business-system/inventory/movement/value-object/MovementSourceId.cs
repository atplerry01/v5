namespace Whycespace.Domain.BusinessSystem.Inventory.Movement;

public readonly record struct MovementSourceId
{
    public Guid Value { get; }

    public MovementSourceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MovementSourceId value must not be empty.", nameof(value));
        Value = value;
    }
}
