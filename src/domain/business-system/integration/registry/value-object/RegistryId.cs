namespace Whycespace.Domain.BusinessSystem.Integration.Registry;

public readonly record struct RegistryId
{
    public Guid Value { get; }

    public RegistryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RegistryId value must not be empty.", nameof(value));

        Value = value;
    }
}
