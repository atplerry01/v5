using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public readonly record struct QuoteBasisId
{
    public Guid Value { get; }

    public QuoteBasisId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "QuoteBasisId cannot be empty.");
        Value = value;
    }
}
