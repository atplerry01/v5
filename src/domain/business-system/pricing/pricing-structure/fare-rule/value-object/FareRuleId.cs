namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public readonly record struct FareRuleId
{
    public Guid Value { get; }

    public FareRuleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FareRuleId value must not be empty.", nameof(value));

        Value = value;
    }
}
