namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Flattened, version-aware DTO for cross-context economic event communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record EconomicEventDTO : IEconomicEventContract
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required int Version { get; init; }
    public required string CorrelationId { get; init; }

    public required Guid SourceEntityId { get; init; }
    public required string SourceEntityType { get; init; }
    public required decimal Amount { get; init; }
    public required string CurrencyCode { get; init; }
    public required string Direction { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
}