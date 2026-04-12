namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public readonly record struct RevenueId
{
    public Guid Value { get; }

    public RevenueId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RevenueId cannot be empty.", nameof(value));
        Value = value;
    }

    public static RevenueId From(Guid value) => new(value);
}
