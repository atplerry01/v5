namespace Whycespace.Domain.ControlSystem.Observability.SystemSignal;

public readonly record struct SystemSignalId
{
    public string Value { get; }

    public SystemSignalId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw SystemSignalErrors.SignalIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw SystemSignalErrors.SignalIdMustBe64HexChars(value);

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
