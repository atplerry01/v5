namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationState;

public readonly record struct ConfigurationStateId
{
    public string Value { get; }

    public ConfigurationStateId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw ConfigurationStateErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw ConfigurationStateErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
