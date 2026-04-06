namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public sealed record CapacityValue
{
    public decimal Value { get; }

    public CapacityValue(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Capacity value cannot be negative.", nameof(value));

        Value = value;
    }

    public CapacityValue Add(CapacityValue other) => new(Value + other.Value);

    public CapacityValue Subtract(CapacityValue other)
    {
        if (other.Value > Value)
            throw new InvalidOperationException("Cannot reduce capacity below zero.");

        return new(Value - other.Value);
    }

    public static CapacityValue Zero => new(0);

    public static implicit operator decimal(CapacityValue cv) => cv.Value;
}
