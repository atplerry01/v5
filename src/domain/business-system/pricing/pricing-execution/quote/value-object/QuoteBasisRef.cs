using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public readonly record struct QuoteBasisRef
{
    public Guid Value { get; }

    public QuoteBasisRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "QuoteBasisRef cannot be empty.");
        Value = value;
    }
}
