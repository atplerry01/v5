using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public readonly record struct LineQuantity
{
    public decimal Value { get; }
    public string Unit { get; }

    public LineQuantity(decimal value, string unit)
    {
        Guard.Against(value <= 0m, "LineQuantity must be positive.");
        Guard.Against(string.IsNullOrWhiteSpace(unit), "LineQuantity unit must not be empty.");

        Value = value;
        Unit = unit!.Trim();
    }
}
