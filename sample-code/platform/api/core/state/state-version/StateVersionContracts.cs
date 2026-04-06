namespace Whycespace.Platform.Api.Core.State.StateVersion;

public sealed record StateVersionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record StateVersionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
