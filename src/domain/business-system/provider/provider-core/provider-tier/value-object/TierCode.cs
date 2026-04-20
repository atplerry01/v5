namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public readonly record struct TierCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public TierCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("TierCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"TierCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
