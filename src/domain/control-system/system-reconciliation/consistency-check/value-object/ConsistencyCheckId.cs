namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ConsistencyCheck;

public readonly record struct ConsistencyCheckId
{
    public string Value { get; }

    public ConsistencyCheckId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw ConsistencyCheckErrors.CheckIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw ConsistencyCheckErrors.CheckIdMustBe64HexChars(value);

        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s)
            if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                return false;
        return true;
    }

    public override string ToString() => Value;
}
