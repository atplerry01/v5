namespace Whycespace.Domain.BusinessSystem.Portfolio.Holding;

public readonly record struct HoldingQuantity
{
    public int Value { get; }

    public HoldingQuantity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("HoldingQuantity must be greater than zero.", nameof(value));

        Value = value;
    }
}
