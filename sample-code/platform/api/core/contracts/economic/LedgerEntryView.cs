namespace Whycespace.Platform.Api.Core.Contracts.Economic;

/// <summary>
/// Read-only ledger entry projection view.
/// Sourced from CQRS projections — no domain access, no aggregates.
/// </summary>
public sealed record LedgerEntryView
{
    public required Guid EntryId { get; init; }
    public required Guid AccountId { get; init; }
    public required decimal Amount { get; init; }
    public required string Type { get; init; }
    public required string Currency { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? Description { get; init; }
    public string? CorrelationId { get; init; }
}
