namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public readonly record struct ProductName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ProductName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ProductName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
