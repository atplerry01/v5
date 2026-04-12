namespace Whycespace.Domain.BusinessSystem.Portfolio.Allocation;

public readonly record struct AllocationWeight
{
    public decimal Value { get; }

    public AllocationWeight(decimal value)
    {
        if (value <= 0m || value > 1m)
            throw new ArgumentException("AllocationWeight must be greater than zero and at most 1.", nameof(value));

        Value = value;
    }
}
