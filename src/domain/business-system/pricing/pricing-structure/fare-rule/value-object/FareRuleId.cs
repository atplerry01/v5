using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public readonly record struct FareRuleId
{
    public Guid Value { get; }

    public FareRuleId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "FareRuleId cannot be empty.");
        Value = value;
    }
}
