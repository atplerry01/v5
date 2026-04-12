namespace Whycespace.Domain.BusinessSystem.Inventory.Replenishment;

public readonly record struct ReplenishmentThreshold
{
    public int Value { get; }

    public ReplenishmentThreshold(int value)
    {
        if (value < 0)
            throw new ArgumentException("ReplenishmentThreshold cannot be negative.", nameof(value));
        Value = value;
    }
}
