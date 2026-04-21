using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public readonly record struct SurchargeId
{
    public Guid Value { get; }

    public SurchargeId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SurchargeId cannot be empty.");
        Value = value;
    }
}
