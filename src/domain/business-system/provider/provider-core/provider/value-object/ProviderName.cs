namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.Provider;

public readonly record struct ProviderName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ProviderName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ProviderName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ProviderName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
