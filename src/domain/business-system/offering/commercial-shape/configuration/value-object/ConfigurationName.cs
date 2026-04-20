namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public readonly record struct ConfigurationName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ConfigurationName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ConfigurationName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ConfigurationName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
