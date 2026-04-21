using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public readonly record struct TierRank
{
    public int Value { get; }

    public TierRank(int value)
    {
        Guard.Against(value < 0, "TierRank must be non-negative.");
        Value = value;
    }
}
