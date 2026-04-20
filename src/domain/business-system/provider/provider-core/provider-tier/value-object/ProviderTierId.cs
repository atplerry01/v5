namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public readonly record struct ProviderTierId
{
    public Guid Value { get; }

    public ProviderTierId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProviderTierId value must not be empty.", nameof(value));

        Value = value;
    }
}
