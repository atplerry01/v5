using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public readonly record struct RateCardName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public RateCardName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "RateCardName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"RateCardName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
