using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public readonly record struct DiscountAmount
{
    public decimal Value { get; }

    public DiscountAmount(decimal value)
    {
        Guard.Against(value < 0m, "DiscountAmount must be non-negative.");
        Value = value;
    }
}
