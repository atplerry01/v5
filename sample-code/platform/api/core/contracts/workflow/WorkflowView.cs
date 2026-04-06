namespace Whycespace.Platform.Api.Core.Contracts.Workflow;

/// <summary>
/// Read-only workflow execution projection view.
/// Exposes workflow status, step-level progress, failure details, and retry state.
/// Sourced from CQRS projections — no domain access, no event replay, no state reconstruction.
/// </summary>
public sealed record WorkflowView
{
    public required Guid WorkflowId { get; init; }
    public required string WorkflowKey { get; init; }
    public required string Status { get; init; }
    public required string Cluster { get; init; }
    public string? CorrelationId { get; init; }
    public Guid? IdentityId { get; init; }
    public required IReadOnlyList<WorkflowStepView> Steps { get; init; }
    public string? FailureReason { get; init; }
    public int RetryCount { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
