namespace Whycespace.Platform.Api.Orchestration.Workflow.Execution;

public sealed record ExecutionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ExecutionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
