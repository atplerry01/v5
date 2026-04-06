namespace Whycespace.Shared.Contracts.Domain.Resource;

/// <summary>
/// Flattened, version-aware DTO for cross-context resource usage communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record ResourceUsageDTO : IResourceEventContract
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required int Version { get; init; }
    public required string CorrelationId { get; init; }

    public required Guid ResourceId { get; init; }
    public required string ResourceType { get; init; }

    public required double UtilizationPercent { get; init; }
    public required double CapacityTotal { get; init; }
    public required double CapacityUsed { get; init; }
    public string? Unit { get; init; }
    public DateTimeOffset? MeasuredAt { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
}
