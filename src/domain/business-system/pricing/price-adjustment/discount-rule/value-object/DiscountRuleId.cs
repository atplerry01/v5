namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public readonly record struct DiscountRuleId
{
    public Guid Value { get; }

    public DiscountRuleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DiscountRuleId value must not be empty.", nameof(value));

        Value = value;
    }
}
