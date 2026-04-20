namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public readonly record struct DiscountRuleName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public DiscountRuleName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("DiscountRuleName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"DiscountRuleName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
