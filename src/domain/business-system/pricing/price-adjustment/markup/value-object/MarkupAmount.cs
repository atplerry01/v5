namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public readonly record struct MarkupAmount
{
    public decimal Value { get; }

    public MarkupAmount(decimal value)
    {
        if (value < 0m)
            throw new ArgumentException("MarkupAmount must be non-negative.", nameof(value));

        Value = value;
    }
}
