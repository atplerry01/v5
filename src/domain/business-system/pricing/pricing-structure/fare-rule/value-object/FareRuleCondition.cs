using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public readonly record struct FareRuleCondition
{
    public const int MaxLength = 2000;

    public string Value { get; }

    public FareRuleCondition(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "FareRuleCondition must not be empty.");
        Guard.Against(value!.Length > MaxLength, $"FareRuleCondition exceeds {MaxLength} characters.");

        Value = value;
    }
}
