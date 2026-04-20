namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public readonly record struct QuoteReference
{
    public const int MaxLength = 64;

    public string Value { get; }

    public QuoteReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("QuoteReference must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"QuoteReference exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
