namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public readonly record struct UsageAmount
{
    public decimal Value { get; }

    public UsageAmount(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("UsageAmount must not be negative.", nameof(value));

        Value = value;
    }
}
