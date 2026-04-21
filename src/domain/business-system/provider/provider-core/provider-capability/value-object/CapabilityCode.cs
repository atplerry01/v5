using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public readonly record struct CapabilityCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public CapabilityCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "CapabilityCode must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"CapabilityCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
