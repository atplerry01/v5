namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public readonly record struct SurchargeCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public SurchargeCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SurchargeCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"SurchargeCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
