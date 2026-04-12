namespace Whycespace.Domain.BusinessSystem.Integration.Endpoint;

public readonly record struct EndpointId
{
    public Guid Value { get; }

    public EndpointId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EndpointId value must not be empty.", nameof(value));
        Value = value;
    }
}
