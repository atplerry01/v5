using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public readonly record struct MarkupId
{
    public Guid Value { get; }

    public MarkupId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MarkupId cannot be empty.");
        Value = value;
    }
}
