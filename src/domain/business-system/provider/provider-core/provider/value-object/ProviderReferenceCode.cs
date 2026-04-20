namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.Provider;

public readonly record struct ProviderReferenceCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public ProviderReferenceCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ProviderReferenceCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ProviderReferenceCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
