namespace Whycespace.Domain.BusinessSystem.Resource.Capacity;

public readonly record struct CapacityLimit
{
    public int Value { get; }

    public CapacityLimit(int value)
    {
        if (value < 0)
            throw new ArgumentException("CapacityLimit must be non-negative.", nameof(value));

        Value = value;
    }
}
