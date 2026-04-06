namespace Whycespace.Domain.SharedKernel.Primitive.Money;

public readonly record struct Amount : IComparable<Amount>
{
    public decimal Value { get; }

    public Amount(decimal value)
    {
        Value = value;
    }

    public Amount Add(Amount other) => new(Value + other.Value);
    public Amount Subtract(Amount other) => new(Value - other.Value);
    public Amount Multiply(decimal factor) => new(Value * factor);

    public static Amount Zero => new(0);

    public static Amount operator +(Amount left, Amount right) => left.Add(right);
    public static Amount operator -(Amount left, Amount right) => left.Subtract(right);
    public static Amount operator *(Amount amount, decimal factor) => amount.Multiply(factor);
    public static bool operator >(Amount left, Amount right) => left.Value > right.Value;
    public static bool operator <(Amount left, Amount right) => left.Value < right.Value;
    public static bool operator >=(Amount left, Amount right) => left.Value >= right.Value;
    public static bool operator <=(Amount left, Amount right) => left.Value <= right.Value;

    public bool IsZero => Value == 0;
    public bool IsPositive => Value > 0;
    public bool IsNegative => Value < 0;

    public int CompareTo(Amount other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}
