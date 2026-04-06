namespace Whycespace.Shared.Primitives.Id;

public abstract class BaseId : IEquatable<BaseId>
{
    public string Value { get; }

    protected BaseId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) =>
        obj is BaseId other && Equals(other);

    public bool Equals(BaseId? other) =>
        other is not null && GetType() == other.GetType() && Value == other.Value;

    public override int GetHashCode() =>
        HashCode.Combine(GetType(), Value);

    public static bool operator ==(BaseId? left, BaseId? right) =>
        Equals(left, right);

    public static bool operator !=(BaseId? left, BaseId? right) =>
        !Equals(left, right);
}
