using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public readonly record struct MarkupAmount
{
    public decimal Value { get; }

    public MarkupAmount(decimal value)
    {
        Guard.Against(value < 0m, "MarkupAmount must be non-negative.");
        Value = value;
    }
}
