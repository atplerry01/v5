namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public readonly record struct MarkupName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public MarkupName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("MarkupName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"MarkupName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
