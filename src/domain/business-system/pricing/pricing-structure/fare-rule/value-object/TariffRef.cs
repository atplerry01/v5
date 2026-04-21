using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public readonly record struct TariffRef
{
    public Guid Value { get; }

    public TariffRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "TariffRef cannot be empty.");
        Value = value;
    }
}
