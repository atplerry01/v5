namespace Whycespace.Domain.BusinessSystem.Integration.Endpoint;

public readonly record struct EndpointUri
{
    public string Value { get; }

    public EndpointUri(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("EndpointUri value must not be empty.", nameof(value));
        Value = value;
    }
}
