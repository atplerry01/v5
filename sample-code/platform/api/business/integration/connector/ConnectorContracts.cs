namespace Whycespace.Platform.Api.Business.Integration.Connector;

public sealed record ConnectorRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ConnectorResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
