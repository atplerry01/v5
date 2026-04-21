using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public readonly record struct TariffId
{
    public Guid Value { get; }

    public TariffId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "TariffId cannot be empty.");
        Value = value;
    }
}
