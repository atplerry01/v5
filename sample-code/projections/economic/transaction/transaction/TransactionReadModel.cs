namespace Whycespace.Projections.Economic;

/// <summary>
/// Transaction history entry projected from transaction.* events.
/// Global ordering: events applied only if newer than current state
/// (Timestamp + Version tiebreaker).
/// </summary>
public sealed record TransactionHistoryReadModel
{
    public required string TransactionId { get; init; }
    public required string Status { get; init; }
    public required decimal Amount { get; init; }
    public required string CurrencyCode { get; init; }
    public string? SourceWalletId { get; init; }
    public string? DestinationWalletId { get; init; }
    public int Version { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
