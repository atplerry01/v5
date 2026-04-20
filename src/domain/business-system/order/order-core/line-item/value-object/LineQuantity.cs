namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public readonly record struct LineQuantity
{
    public decimal Value { get; }
    public string Unit { get; }

    public LineQuantity(decimal value, string unit)
    {
        if (value <= 0m)
            throw new ArgumentException("LineQuantity must be positive.", nameof(value));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("LineQuantity unit must not be empty.", nameof(unit));

        Value = value;
        Unit = unit.Trim();
    }
}
