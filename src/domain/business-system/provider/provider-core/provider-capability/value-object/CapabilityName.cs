using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public readonly record struct CapabilityName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public CapabilityName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "CapabilityName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"CapabilityName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
