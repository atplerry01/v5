namespace Whycespace.Shared.Contracts.Platform.Responses;

/// <summary>
/// Flattened, version-aware response DTO for workflow instance status.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record WorkflowStatusDTO
{
    public required int Version { get; init; }

    public required Guid WorkflowInstanceId { get; init; }
    public required string WorkflowType { get; init; }
    public required string CurrentState { get; init; }
    public required bool IsCompleted { get; init; }
    public required DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }
    public string? CurrentStepName { get; init; }
    public Guid? CurrentStepId { get; init; }
    public Guid? InitiatorIdentityId { get; init; }
    public string? CorrelationId { get; init; }
}
