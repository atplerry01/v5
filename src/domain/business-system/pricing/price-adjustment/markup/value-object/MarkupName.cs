using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public readonly record struct MarkupName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public MarkupName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MarkupName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"MarkupName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
