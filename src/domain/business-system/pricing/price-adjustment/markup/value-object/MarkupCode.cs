namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public readonly record struct MarkupCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public MarkupCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("MarkupCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"MarkupCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
