using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public readonly record struct SurchargeAmount
{
    public decimal Value { get; }

    public SurchargeAmount(decimal value)
    {
        Guard.Against(value < 0m, "SurchargeAmount must be non-negative.");
        Value = value;
    }
}
