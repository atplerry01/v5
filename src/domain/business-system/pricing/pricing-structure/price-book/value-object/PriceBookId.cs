namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public readonly record struct PriceBookId
{
    public Guid Value { get; }

    public PriceBookId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PriceBookId value must not be empty.", nameof(value));

        Value = value;
    }
}
