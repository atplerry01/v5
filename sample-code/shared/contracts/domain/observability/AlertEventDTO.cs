namespace Whycespace.Shared.Contracts.Domain.Observability;

/// <summary>
/// Flattened, version-aware DTO for cross-context alert event communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record AlertEventDTO : IObservabilityEventContract
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required int Version { get; init; }
    public required string CorrelationId { get; init; }

    public required Guid SourceId { get; init; }
    public required string SourceType { get; init; }
    public required string Severity { get; init; }

    public required Guid AlertId { get; init; }
    public required string AlertCode { get; init; }
    public required string AlertStatus { get; init; }
    public string? ThresholdBreached { get; init; }
    public double? ThresholdValue { get; init; }
    public double? ActualValue { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
}
