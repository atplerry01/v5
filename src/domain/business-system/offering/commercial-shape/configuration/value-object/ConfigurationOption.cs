using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public readonly record struct ConfigurationOption
{
    public const int KeyMaxLength = 64;
    public const int ValueMaxLength = 512;

    public string Key { get; }
    public string Value { get; }

    public ConfigurationOption(string key, string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(key), "ConfigurationOption key must not be empty.");
        Guard.Against(key!.Trim().Length > KeyMaxLength, $"ConfigurationOption key exceeds {KeyMaxLength} characters.");
        Guard.Against(value is null, "ConfigurationOption value must not be null.");
        Guard.Against(value!.Length > ValueMaxLength, $"ConfigurationOption value exceeds {ValueMaxLength} characters.");

        Key = key.Trim();
        Value = value;
    }
}
