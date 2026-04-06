namespace Whycespace.Shared.Contracts.Domain.Workflow;

/// <summary>
/// Flattened, version-aware DTO for cross-context workflow event communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record WorkflowEventDTO : IWorkflowEventContract
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required int Version { get; init; }
    public required string CorrelationId { get; init; }

    public required Guid WorkflowInstanceId { get; init; }
    public required string WorkflowType { get; init; }
    public required string CurrentState { get; init; }
    public string? PreviousState { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
    public Guid? StepId { get; init; }
    public string? StepName { get; init; }
    public Guid? TransitionTriggerId { get; init; }
}
