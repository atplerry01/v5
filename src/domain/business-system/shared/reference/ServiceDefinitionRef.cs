namespace Whycespace.Domain.BusinessSystem.Shared.Reference;

public readonly record struct ServiceDefinitionRef
{
    public Guid Value { get; }

    public ServiceDefinitionRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceDefinitionRef value must not be empty.", nameof(value));

        Value = value;
    }
}
