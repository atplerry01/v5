namespace Whycespace.Platform.Api.Orchestration.Workflow.Stage;

public sealed record StageRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record StageResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
