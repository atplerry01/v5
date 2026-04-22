namespace Whycespace.Domain.ControlSystem.Observability.SystemTrace;

public readonly record struct SystemTraceId
{
    public string Value { get; }

    public SystemTraceId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw SystemTraceErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw SystemTraceErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
