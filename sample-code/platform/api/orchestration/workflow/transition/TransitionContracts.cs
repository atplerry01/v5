namespace Whycespace.Platform.Api.Orchestration.Workflow.Transition;

public sealed record TransitionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TransitionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
