namespace Whycespace.Platform.Api.Core.State.StateSnapshot;

public sealed record StateSnapshotRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record StateSnapshotResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
