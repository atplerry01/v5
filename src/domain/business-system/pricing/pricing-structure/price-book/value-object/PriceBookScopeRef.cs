using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public readonly record struct PriceBookScopeRef
{
    public Guid Value { get; }

    public PriceBookScopeRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PriceBookScopeRef cannot be empty.");
        Value = value;
    }
}
