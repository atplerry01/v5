namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public readonly record struct QuoteId
{
    public Guid Value { get; }

    public QuoteId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("QuoteId value must not be empty.", nameof(value));

        Value = value;
    }
}
