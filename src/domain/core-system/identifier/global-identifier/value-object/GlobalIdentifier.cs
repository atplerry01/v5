namespace Whycespace.Domain.CoreSystem.Identifier.GlobalIdentifier;

public readonly record struct GlobalIdentifier : IComparable<GlobalIdentifier>
{
    private const int RequiredLength = 64;

    public string Value { get; }

    public GlobalIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw GlobalIdentifierErrors.ValueMustNotBeEmpty();

        if (value.Length != RequiredLength || !IsLowercaseHex(value))
            throw GlobalIdentifierErrors.ValueMustBe64LowercaseHexChars(value);

        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s)
            if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                return false;
        return true;
    }

    public int CompareTo(GlobalIdentifier other) =>
        string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(GlobalIdentifier left, GlobalIdentifier right) =>
        left.CompareTo(right) < 0;
    public static bool operator >(GlobalIdentifier left, GlobalIdentifier right) =>
        left.CompareTo(right) > 0;
    public static bool operator <=(GlobalIdentifier left, GlobalIdentifier right) =>
        left.CompareTo(right) <= 0;
    public static bool operator >=(GlobalIdentifier left, GlobalIdentifier right) =>
        left.CompareTo(right) >= 0;

    public override string ToString() => Value;
}
