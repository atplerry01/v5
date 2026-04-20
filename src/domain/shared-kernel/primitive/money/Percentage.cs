using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.SharedKernel.Primitive.Money;

public readonly record struct Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        Guard.Against(value < 0m, "Percentage cannot be negative.");
        Guard.Against(value > 1m, "Percentage cannot exceed 1 (use ratio form: 0.25 for 25%).");
        Value = value;
    }
}
