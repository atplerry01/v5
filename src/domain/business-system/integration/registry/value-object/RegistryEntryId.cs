namespace Whycespace.Domain.BusinessSystem.Integration.Registry;

public readonly record struct RegistryEntryId
{
    public Guid Value { get; }

    public RegistryEntryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RegistryEntryId value must not be empty.", nameof(value));

        Value = value;
    }
}
