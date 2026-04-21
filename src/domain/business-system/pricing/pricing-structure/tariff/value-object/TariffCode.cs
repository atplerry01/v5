using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public readonly record struct TariffCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public TariffCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "TariffCode must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"TariffCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
