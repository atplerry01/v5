namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public readonly record struct QuoteBasisContext
{
    public const int MaxLength = 2000;

    public string Value { get; }

    public QuoteBasisContext(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("QuoteBasisContext must not be empty.", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"QuoteBasisContext exceeds {MaxLength} characters.", nameof(value));

        Value = value;
    }
}
