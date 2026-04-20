namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.Provider;

public readonly record struct ProviderId
{
    public Guid Value { get; }

    public ProviderId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProviderId value must not be empty.", nameof(value));

        Value = value;
    }
}
