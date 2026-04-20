namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public readonly record struct FareRuleCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public FareRuleCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("FareRuleCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"FareRuleCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
