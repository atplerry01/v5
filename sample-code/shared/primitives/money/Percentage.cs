namespace Whycespace.Shared.Primitives.Money;

public readonly record struct Percentage : IComparable<Percentage>
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        Value = value;
    }

    public decimal ApplyTo(decimal amount) => amount * Value / 100m;

    public static Percentage Zero => new(0);
    public static Percentage Full => new(100);

    public static bool operator >(Percentage left, Percentage right) => left.Value > right.Value;
    public static bool operator <(Percentage left, Percentage right) => left.Value < right.Value;
    public static bool operator >=(Percentage left, Percentage right) => left.Value >= right.Value;
    public static bool operator <=(Percentage left, Percentage right) => left.Value <= right.Value;

    public int CompareTo(Percentage other) => Value.CompareTo(other.Value);

    public override string ToString() => $"{Value}%";
}
