namespace Whycespace.Shared.Contracts.Domain.Operational;

/// <summary>
/// Flattened, version-aware DTO for cross-context mobility/ride event communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record RideDTO : IOperationalEventContract
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

    public required Guid JobId { get; init; }
    public required double StartLatitude { get; init; }
    public required double StartLongitude { get; init; }
    public string? StartLabel { get; init; }
    public required double EndLatitude { get; init; }
    public required double EndLongitude { get; init; }
    public string? EndLabel { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? CancellationReason { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
}
