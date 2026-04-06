namespace Whycespace.Shared.Contracts.Domain.Policy;

/// <summary>
/// Flattened, version-aware DTO for cross-context policy decision communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record PolicyDecisionDTO : IPolicyEventContract
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required int Version { get; init; }
    public required string CorrelationId { get; init; }

    public required Guid PolicyId { get; init; }
    public required string PolicyType { get; init; }
    public required string EvaluationResult { get; init; }
    public string? ViolationCode { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
    public Guid? TargetEntityId { get; init; }
    public string? TargetEntityType { get; init; }
    public string? Severity { get; init; }
    public string? RemediationAction { get; init; }
}
