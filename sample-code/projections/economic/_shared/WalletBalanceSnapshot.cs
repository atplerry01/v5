namespace Whycespace.Projections.Economic;

/// <summary>
/// Point-in-time snapshot of wallet balance for rebuild acceleration.
/// On rebuild: load snapshot, replay only events after LastEventTimestamp.
/// </summary>
public sealed record WalletBalanceSnapshot
{
    public required string WalletId { get; init; }
    public required string CurrencyCode { get; init; }
    public decimal Balance { get; init; }
    public decimal TotalCredits { get; init; }
    public decimal TotalDebits { get; init; }
    public int EntryCount { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
    public DateTimeOffset SnapshotTakenAt { get; init; }

    public string CompositeKey => WalletBalanceReadModel.CompositeKey(WalletId, CurrencyCode);
}
