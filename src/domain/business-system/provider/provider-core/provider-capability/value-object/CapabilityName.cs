namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public readonly record struct CapabilityName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public CapabilityName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CapabilityName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"CapabilityName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
