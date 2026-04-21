using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public readonly record struct TierName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public TierName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "TierName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"TierName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
