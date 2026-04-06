namespace Whycespace.Platform.Api.Business.Integration.EventBridge;

public sealed record EventBridgeRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EventBridgeResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
