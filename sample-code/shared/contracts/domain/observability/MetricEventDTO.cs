namespace Whycespace.Shared.Contracts.Domain.Observability;

/// <summary>
/// Flattened, version-aware DTO for cross-context metric event communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record MetricEventDTO : IObservabilityEventContract
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required int Version { get; init; }
    public required string CorrelationId { get; init; }

    public required Guid SourceId { get; init; }
    public required string SourceType { get; init; }
    public required string Severity { get; init; }

    public required Guid MetricId { get; init; }
    public required string MetricName { get; init; }
    public required double MetricValue { get; init; }
    public string? Unit { get; init; }
    public string? Dimension { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
}
