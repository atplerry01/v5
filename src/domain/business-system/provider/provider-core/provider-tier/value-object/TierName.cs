namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public readonly record struct TierName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public TierName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("TierName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"TierName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
