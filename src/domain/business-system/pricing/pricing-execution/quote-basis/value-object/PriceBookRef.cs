namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public readonly record struct PriceBookRef
{
    public Guid Value { get; }

    public PriceBookRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PriceBookRef value must not be empty.", nameof(value));

        Value = value;
    }
}
