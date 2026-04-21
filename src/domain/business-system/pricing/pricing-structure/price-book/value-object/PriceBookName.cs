using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public readonly record struct PriceBookName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public PriceBookName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "PriceBookName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"PriceBookName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
