namespace Whycespace.Domain.ControlSystem.Observability.SystemAlert;

public readonly record struct SystemAlertId
{
    public string Value { get; }

    public SystemAlertId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw SystemAlertErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw SystemAlertErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
