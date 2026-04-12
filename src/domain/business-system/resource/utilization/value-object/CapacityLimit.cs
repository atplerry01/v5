namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public readonly record struct CapacityLimit
{
    public int Value { get; }

    public CapacityLimit(int value)
    {
        if (value <= 0)
            throw new ArgumentException("CapacityLimit must be greater than zero.", nameof(value));

        Value = value;
    }
}
