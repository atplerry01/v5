namespace Whycespace.Domain.ControlSystem.Scheduling.SystemJob;

public readonly record struct SystemJobId
{
    public string Value { get; }

    public SystemJobId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw SystemJobErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw SystemJobErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
