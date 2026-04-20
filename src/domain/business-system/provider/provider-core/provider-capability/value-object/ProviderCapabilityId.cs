namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public readonly record struct ProviderCapabilityId
{
    public Guid Value { get; }

    public ProviderCapabilityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProviderCapabilityId value must not be empty.", nameof(value));

        Value = value;
    }
}
