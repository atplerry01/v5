using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public readonly record struct PriceBookId
{
    public Guid Value { get; }

    public PriceBookId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PriceBookId cannot be empty.");
        Value = value;
    }
}
