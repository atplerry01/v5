using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public readonly record struct FareRuleCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public FareRuleCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "FareRuleCode must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"FareRuleCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
