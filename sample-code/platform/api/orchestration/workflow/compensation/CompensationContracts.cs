namespace Whycespace.Platform.Api.Orchestration.Workflow.Compensation;

public sealed record CompensationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CompensationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
