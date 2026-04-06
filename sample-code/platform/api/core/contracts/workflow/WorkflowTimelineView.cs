namespace Whycespace.Platform.Api.Core.Contracts.Workflow;

/// <summary>
/// Read-only workflow timeline projection view.
/// Provides a chronological list of events for a workflow execution.
/// Sourced from CQRS projections — no event store access, no event replay.
/// </summary>
public sealed record WorkflowTimelineView
{
    public required Guid WorkflowId { get; init; }
    public required string WorkflowKey { get; init; }
    public required IReadOnlyList<WorkflowEventView> Events { get; init; }
}
