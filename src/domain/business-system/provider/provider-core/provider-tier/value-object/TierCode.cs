using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public readonly record struct TierCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public TierCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "TierCode must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"TierCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
