namespace Whycespace.Shared.Contracts.Domain.Operational;

/// <summary>
/// Flattened, version-aware DTO for cross-context incident event communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record IncidentDTO : IOperationalEventContract
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required int Version { get; init; }
    public required string CorrelationId { get; init; }

    public required Guid SourceEntityId { get; init; }
    public required string SourceEntityType { get; init; }
    public required string Status { get; init; }
    public Guid? AssignedOperatorId { get; init; }

    public required Guid IncidentId { get; init; }
    public required string IncidentType { get; init; }
    public required string Severity { get; init; }
    public required Guid AffectedEntityId { get; init; }
    public required string Description { get; init; }
    public Guid? InvestigatorIdentityId { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
}
