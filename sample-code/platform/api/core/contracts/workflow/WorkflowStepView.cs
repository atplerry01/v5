namespace Whycespace.Platform.Api.Core.Contracts.Workflow;

/// <summary>
/// Read-only workflow step projection view.
/// Represents a single step within a workflow execution.
/// Sourced from CQRS projections — no domain access, no event replay.
/// </summary>
public sealed record WorkflowStepView
{
    public required string StepId { get; init; }
    public required string StepName { get; init; }
    public required string Status { get; init; }
    public int Order { get; init; }
    public string? Error { get; init; }
    public int RetryCount { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
