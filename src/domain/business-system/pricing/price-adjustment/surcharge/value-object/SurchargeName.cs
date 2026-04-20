namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public readonly record struct SurchargeName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public SurchargeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SurchargeName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"SurchargeName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
