namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public readonly record struct MarkupId
{
    public Guid Value { get; }

    public MarkupId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MarkupId value must not be empty.", nameof(value));

        Value = value;
    }
}
