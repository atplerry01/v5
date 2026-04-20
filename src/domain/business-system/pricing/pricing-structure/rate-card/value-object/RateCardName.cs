namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public readonly record struct RateCardName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public RateCardName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("RateCardName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"RateCardName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
