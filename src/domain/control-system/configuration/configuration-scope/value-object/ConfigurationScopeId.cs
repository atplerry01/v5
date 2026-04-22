namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationScope;

public readonly record struct ConfigurationScopeId
{
    public string Value { get; }

    public ConfigurationScopeId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw ConfigurationScopeErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw ConfigurationScopeErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
