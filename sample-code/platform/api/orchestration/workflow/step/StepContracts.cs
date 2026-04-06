namespace Whycespace.Platform.Api.Orchestration.Workflow.Step;

public sealed record StepRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record StepResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
