namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public readonly record struct FareRuleCondition
{
    public const int MaxLength = 2000;

    public string Value { get; }

    public FareRuleCondition(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("FareRuleCondition must not be empty.", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"FareRuleCondition exceeds {MaxLength} characters.", nameof(value));

        Value = value;
    }
}
