namespace Whycespace.Domain.ControlSystem.Observability.SystemHealth;

public readonly record struct SystemHealthId
{
    public string Value { get; }

    public SystemHealthId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw SystemHealthErrors.HealthIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw SystemHealthErrors.HealthIdMustBe64HexChars(value);

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
