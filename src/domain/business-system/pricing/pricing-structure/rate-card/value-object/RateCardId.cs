using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public readonly record struct RateCardId
{
    public Guid Value { get; }

    public RateCardId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "RateCardId cannot be empty.");
        Value = value;
    }
}
