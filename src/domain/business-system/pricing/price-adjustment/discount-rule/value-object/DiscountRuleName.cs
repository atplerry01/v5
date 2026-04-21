using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public readonly record struct DiscountRuleName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public DiscountRuleName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "DiscountRuleName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"DiscountRuleName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
