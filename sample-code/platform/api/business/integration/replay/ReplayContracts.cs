namespace Whycespace.Platform.Api.Business.Integration.Replay;

public sealed record ReplayRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReplayResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
