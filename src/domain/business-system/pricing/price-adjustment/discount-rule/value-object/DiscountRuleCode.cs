using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public readonly record struct DiscountRuleCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public DiscountRuleCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "DiscountRuleCode must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"DiscountRuleCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
