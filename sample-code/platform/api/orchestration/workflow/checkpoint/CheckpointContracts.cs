namespace Whycespace.Platform.Api.Orchestration.Workflow.Checkpoint;

public sealed record CheckpointRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CheckpointResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
