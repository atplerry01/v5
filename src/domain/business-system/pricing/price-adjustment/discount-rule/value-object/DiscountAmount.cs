namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public readonly record struct DiscountAmount
{
    public decimal Value { get; }

    public DiscountAmount(decimal value)
    {
        if (value < 0m)
            throw new ArgumentException("DiscountAmount must be non-negative.", nameof(value));

        Value = value;
    }
}
