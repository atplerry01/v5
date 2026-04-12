namespace Whycespace.Domain.BusinessSystem.Integration.Endpoint;

public sealed class EndpointDefinition
{
    public EndpointUri Uri { get; }
    public string Method { get; }
    public string Protocol { get; }

    public EndpointDefinition(EndpointUri uri, string method, string protocol)
    {
        if (string.IsNullOrWhiteSpace(method))
            throw new ArgumentException("Method must not be empty.", nameof(method));

        if (string.IsNullOrWhiteSpace(protocol))
            throw new ArgumentException("Protocol must not be empty.", nameof(protocol));

        Uri = uri;
        Method = method;
        Protocol = protocol;
    }
}
