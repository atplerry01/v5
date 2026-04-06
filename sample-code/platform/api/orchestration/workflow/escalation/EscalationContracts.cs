namespace Whycespace.Platform.Api.Orchestration.Workflow.Escalation;

public sealed record EscalationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EscalationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
