namespace Whycespace.Domain.BusinessSystem.Inventory.Movement;

public readonly record struct MovementTargetId
{
    public Guid Value { get; }

    public MovementTargetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MovementTargetId value must not be empty.", nameof(value));
        Value = value;
    }
}
