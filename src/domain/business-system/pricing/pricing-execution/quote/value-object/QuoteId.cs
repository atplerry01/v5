using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public readonly record struct QuoteId
{
    public Guid Value { get; }

    public QuoteId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "QuoteId cannot be empty.");
        Value = value;
    }
}
