namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationDefinition;

public readonly record struct ConfigurationDefinitionId
{
    public string Value { get; }

    public ConfigurationDefinitionId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw ConfigurationDefinitionErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw ConfigurationDefinitionErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
