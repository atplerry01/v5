namespace Whycespace.Shared.Contracts.Platform.Commands;

/// <summary>
/// Flattened command DTO for starting a workflow instance.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record StartWorkflowCommandDTO
{
    public required string CommandId { get; init; }
    public required int Version { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    public required string WorkflowType { get; init; }
    public required Guid InitiatorIdentityId { get; init; }
    public string? InitialState { get; init; }
    public string? CorrelationId { get; init; }
    public string? IdempotencyKey { get; init; }
    public Dictionary<string, string>? Parameters { get; init; }
}
