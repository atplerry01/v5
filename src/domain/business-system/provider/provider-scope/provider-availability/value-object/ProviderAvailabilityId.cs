namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public readonly record struct ProviderAvailabilityId
{
    public Guid Value { get; }

    public ProviderAvailabilityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProviderAvailabilityId value must not be empty.", nameof(value));

        Value = value;
    }
}
