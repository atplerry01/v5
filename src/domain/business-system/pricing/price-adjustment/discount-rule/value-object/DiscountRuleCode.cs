namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public readonly record struct DiscountRuleCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public DiscountRuleCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("DiscountRuleCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"DiscountRuleCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
