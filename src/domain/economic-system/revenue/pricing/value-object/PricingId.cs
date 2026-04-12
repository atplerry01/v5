namespace Whycespace.Domain.EconomicSystem.Revenue.Pricing;

public readonly record struct PricingId
{
    public Guid Value { get; }

    public PricingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PricingId cannot be empty.", nameof(value));
        Value = value;
    }

    public static PricingId From(Guid value) => new(value);
}
