namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public readonly record struct CapabilityCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public CapabilityCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CapabilityCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"CapabilityCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
