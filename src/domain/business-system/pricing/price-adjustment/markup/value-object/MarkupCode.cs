using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public readonly record struct MarkupCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public MarkupCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MarkupCode must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"MarkupCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
