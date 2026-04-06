namespace Whycespace.Platform.Api.Business.Integration.Client;

public sealed record ClientRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ClientResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
