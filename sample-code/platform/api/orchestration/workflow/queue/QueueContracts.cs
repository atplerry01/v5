namespace Whycespace.Platform.Api.Orchestration.Workflow.Queue;

public sealed record QueueRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record QueueResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
