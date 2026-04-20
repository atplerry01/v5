namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public readonly record struct ConfigurationOption
{
    public const int KeyMaxLength = 64;
    public const int ValueMaxLength = 512;

    public string Key { get; }
    public string Value { get; }

    public ConfigurationOption(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("ConfigurationOption key must not be empty.", nameof(key));

        if (key.Trim().Length > KeyMaxLength)
            throw new ArgumentException($"ConfigurationOption key exceeds {KeyMaxLength} characters.", nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        if (value.Length > ValueMaxLength)
            throw new ArgumentException($"ConfigurationOption value exceeds {ValueMaxLength} characters.", nameof(value));

        Key = key.Trim();
        Value = value;
    }
}
