namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public readonly record struct TariffName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public TariffName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("TariffName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"TariffName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
