namespace Whycespace.Platform.Api.Core.Contracts.Workflow;

/// <summary>
/// Read-only workflow event projection view.
/// Represents a single event in the workflow timeline.
/// Sourced from CQRS projections — no event replay, no event store access.
/// </summary>
public sealed record WorkflowEventView
{
    public required string EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? StepId { get; init; }
    public string? Description { get; init; }
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
