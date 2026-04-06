namespace Whycespace.Platform.Api.Business.Integration.Endpoint;

public sealed record EndpointRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EndpointResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
