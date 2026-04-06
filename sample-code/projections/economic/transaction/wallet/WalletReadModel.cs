namespace Whycespace.Projections.Economic;

/// <summary>
/// Wallet balance derived ONLY from LedgerEntryRecordedEvent.
/// Balance = SUM(credits) - SUM(debits).
/// Key = WalletId + CurrencyCode (multi-currency safe).
/// Never sourced from wallet aggregate state.
/// </summary>
public sealed record WalletBalanceReadModel
{
    public required string WalletId { get; init; }
    public required string CurrencyCode { get; init; }
    public decimal TotalCredits { get; init; }
    public decimal TotalDebits { get; init; }
    public decimal Balance => TotalCredits - TotalDebits;
    public int EntryCount { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }

    public static string CompositeKey(string walletId, string currencyCode)
        => $"{walletId}:{currencyCode}";
}
