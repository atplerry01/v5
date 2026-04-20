namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public readonly record struct BundleName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public BundleName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("BundleName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"BundleName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
