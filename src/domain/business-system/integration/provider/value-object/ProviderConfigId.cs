namespace Whycespace.Domain.BusinessSystem.Integration.Provider;

public readonly record struct ProviderConfigId
{
    public Guid Value { get; }

    public ProviderConfigId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProviderConfigId value must not be empty.", nameof(value));
        Value = value;
    }
}
