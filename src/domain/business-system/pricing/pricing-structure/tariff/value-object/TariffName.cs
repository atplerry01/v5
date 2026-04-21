using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public readonly record struct TariffName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public TariffName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "TariffName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"TariffName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
