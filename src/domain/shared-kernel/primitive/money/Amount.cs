using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.SharedKernel.Primitive.Money;

public readonly record struct Amount
{
    public decimal Value { get; }

    public Amount(decimal value)
    {
        Guard.Against(value == decimal.MinValue, "Amount cannot be decimal.MinValue (sentinel).");
        Guard.Against(value == decimal.MaxValue, "Amount cannot be decimal.MaxValue (sentinel).");
        Value = value;
    }
}
