namespace Whycespace.Platform.Api.Core.State.StateProjection;

public sealed record StateProjectionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record StateProjectionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
