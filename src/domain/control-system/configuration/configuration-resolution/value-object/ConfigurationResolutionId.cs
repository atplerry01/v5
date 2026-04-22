namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationResolution;

public readonly record struct ConfigurationResolutionId
{
    public string Value { get; }

    public ConfigurationResolutionId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw ConfigurationResolutionErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw ConfigurationResolutionErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
