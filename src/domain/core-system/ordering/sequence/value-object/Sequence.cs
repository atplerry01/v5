namespace Whycespace.Domain.CoreSystem.Ordering.Sequence;

public readonly record struct Sequence : IComparable<Sequence>
{
    public long Value { get; }

    public Sequence(long value)
    {
        if (value < 0)
            throw SequenceErrors.ValueMustBeNonNegative(value);

        Value = value;
    }

    public static Sequence Zero => new(0);

    public Sequence Next() => new(Value + 1);

    public int CompareTo(Sequence other) => Value.CompareTo(other.Value);

    public bool Precedes(Sequence other) => Value < other.Value;
    public bool Follows(Sequence other) => Value > other.Value;

    public static bool operator <(Sequence left, Sequence right) => left.Value < right.Value;
    public static bool operator >(Sequence left, Sequence right) => left.Value > right.Value;
    public static bool operator <=(Sequence left, Sequence right) => left.Value <= right.Value;
    public static bool operator >=(Sequence left, Sequence right) => left.Value >= right.Value;

    public override string ToString() => Value.ToString();
}
