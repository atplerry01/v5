namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public readonly record struct PriceBookScopeRef
{
    public Guid Value { get; }

    public PriceBookScopeRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PriceBookScopeRef value must not be empty.", nameof(value));

        Value = value;
    }
}
