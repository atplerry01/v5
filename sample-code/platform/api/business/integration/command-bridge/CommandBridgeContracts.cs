namespace Whycespace.Platform.Api.Business.Integration.CommandBridge;

public sealed record CommandBridgeRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CommandBridgeResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
