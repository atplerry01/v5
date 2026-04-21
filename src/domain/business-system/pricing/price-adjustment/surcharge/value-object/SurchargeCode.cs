using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public readonly record struct SurchargeCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public SurchargeCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "SurchargeCode must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"SurchargeCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
