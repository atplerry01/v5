namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public readonly record struct TariffCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public TariffCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("TariffCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"TariffCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
