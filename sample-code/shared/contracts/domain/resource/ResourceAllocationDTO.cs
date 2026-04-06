namespace Whycespace.Shared.Contracts.Domain.Resource;

/// <summary>
/// Flattened, version-aware DTO for cross-context resource allocation communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record ResourceAllocationDTO : IResourceEventContract
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required int Version { get; init; }
    public required string CorrelationId { get; init; }

    public required Guid ResourceId { get; init; }
    public required string ResourceType { get; init; }

    public required Guid AllocationId { get; init; }
    public required Guid AllocatedToId { get; init; }
    public required string AllocatedToType { get; init; }
    public required double QuantityAllocated { get; init; }
    public string? Unit { get; init; }
    public string? AllocationStatus { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
}
