using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public readonly record struct DiscountRuleId
{
    public Guid Value { get; }

    public DiscountRuleId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DiscountRuleId cannot be empty.");
        Value = value;
    }
}
