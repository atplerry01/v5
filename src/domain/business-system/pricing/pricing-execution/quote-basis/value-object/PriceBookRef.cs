using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public readonly record struct PriceBookRef
{
    public Guid Value { get; }

    public PriceBookRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PriceBookRef cannot be empty.");
        Value = value;
    }
}
