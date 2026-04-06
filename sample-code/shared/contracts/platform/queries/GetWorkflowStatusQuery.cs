namespace Whycespace.Shared.Contracts.Platform.Queries;

/// <summary>
/// Flattened, version-aware query DTO for retrieving workflow instance status.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record GetWorkflowStatusQuery
{
    public required int Version { get; init; }
    public required Guid WorkflowInstanceId { get; init; }
    public string? CorrelationId { get; init; }
}
