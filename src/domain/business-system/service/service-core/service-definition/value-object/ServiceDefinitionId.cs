namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public readonly record struct ServiceDefinitionId
{
    public Guid Value { get; }

    public ServiceDefinitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceDefinitionId value must not be empty.", nameof(value));

        Value = value;
    }
}
