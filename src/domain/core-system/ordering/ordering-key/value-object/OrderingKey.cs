namespace Whycespace.Domain.CoreSystem.Ordering.OrderingKey;

public readonly record struct OrderingKey : IComparable<OrderingKey>
{
    public string Value { get; }

    public OrderingKey(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw OrderingKeyErrors.ValueMustNotBeEmpty();

        Value = value;
    }

    public int CompareTo(OrderingKey other) =>
        string.Compare(Value, other.Value, StringComparison.Ordinal);

    public bool Precedes(OrderingKey other) => CompareTo(other) < 0;
    public bool Follows(OrderingKey other) => CompareTo(other) > 0;

    public static bool operator <(OrderingKey left, OrderingKey right) => left.CompareTo(right) < 0;
    public static bool operator >(OrderingKey left, OrderingKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(OrderingKey left, OrderingKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(OrderingKey left, OrderingKey right) => left.CompareTo(right) >= 0;

    public override string ToString() => Value;
}
