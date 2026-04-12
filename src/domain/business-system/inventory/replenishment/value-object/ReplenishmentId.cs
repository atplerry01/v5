namespace Whycespace.Domain.BusinessSystem.Inventory.Replenishment;

public readonly record struct ReplenishmentId
{
    public Guid Value { get; }

    public ReplenishmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ReplenishmentId value must not be empty.", nameof(value));
        Value = value;
    }
}
