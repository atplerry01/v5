using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public readonly record struct SurchargeName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public SurchargeName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "SurchargeName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"SurchargeName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
