namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public readonly record struct TierRank
{
    public int Value { get; }

    public TierRank(int value)
    {
        if (value < 0)
            throw new ArgumentException("TierRank must be non-negative.", nameof(value));

        Value = value;
    }
}
