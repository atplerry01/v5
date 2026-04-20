namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public readonly record struct QuoteBasisId
{
    public Guid Value { get; }

    public QuoteBasisId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("QuoteBasisId value must not be empty.", nameof(value));

        Value = value;
    }
}
