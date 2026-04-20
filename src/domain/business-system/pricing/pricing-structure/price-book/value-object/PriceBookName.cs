namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public readonly record struct PriceBookName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public PriceBookName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PriceBookName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"PriceBookName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
