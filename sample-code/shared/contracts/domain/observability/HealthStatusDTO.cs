namespace Whycespace.Shared.Contracts.Domain.Observability;

/// <summary>
/// Flattened, version-aware DTO for cross-context health status communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record HealthStatusDTO : IObservabilityEventContract
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required int Version { get; init; }
    public required string CorrelationId { get; init; }

    public required Guid SourceId { get; init; }
    public required string SourceType { get; init; }
    public required string Severity { get; init; }

    public required Guid ComponentId { get; init; }
    public required string ComponentName { get; init; }
    public required string HealthState { get; init; }
    public DateTimeOffset? LastCheckedAt { get; init; }
    public string? DiagnosticCode { get; init; }
    public string? Details { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
}
