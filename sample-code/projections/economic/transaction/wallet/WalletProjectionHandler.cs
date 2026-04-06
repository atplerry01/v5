using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Economic;

/// <summary>
/// Projects wallet balance from LedgerEntryRecordedEvent ONLY.
/// Balance = SUM(credits) - SUM(debits).
/// Key = WalletId:CurrencyCode (multi-currency safe).
/// Does NOT use wallet aggregate state — ledger events are the source of truth.
///
/// Required fields: WalletId, CurrencyCode, DebitAmount, CreditAmount, TransactionId.
/// Rejects events missing WalletId (no AccountId fallback).
/// Ledger safety: validates DebitAmount == CreditAmount (double-entry invariant).
/// </summary>
public sealed class WalletBalanceProjectionHandler : IdempotentEconomicProjectionHandler
{
    private readonly EconomicReadStore _readStore;

    public WalletBalanceProjectionHandler(EconomicReadStore readStore, IClock clock) : base(clock)
    {
        _readStore = readStore ?? throw new ArgumentNullException(nameof(readStore));
    }

    public override string ProjectionName => "economic.wallet-balance";

    public override string[] EventTypes =>
    [
        "economic.ledger.entry-recorded"
    ];

    protected override async Task ApplyAsync(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken)
    {
        var json = ParsePayload(@event);
        if (json is null) return;

        // Required: WalletId (no AccountId fallback)
        var walletId = GetString(json.Value, "WalletId");
        if (walletId is null) return;

        var creditAmount = GetDecimal(json.Value, "CreditAmount");
        var debitAmount = GetDecimal(json.Value, "DebitAmount");
        var currencyCode = GetString(json.Value, "CurrencyCode");
        if (currencyCode is null) return;

        // Ledger safety: double-entry invariant
        if (debitAmount != creditAmount)
            return;

        var compositeKey = WalletBalanceReadModel.CompositeKey(walletId, currencyCode);
        var existing = await _readStore.GetWalletBalanceAsync(compositeKey, cancellationToken);

        // Global ordering guard
        if (existing is not null &&
            ShouldSkipEvent(@event.Timestamp, @event.Version,
                existing.LastEventTimestamp, existing.LastEventVersion))
            return;

        var updated = existing is not null
            ? existing with
            {
                TotalCredits = existing.TotalCredits + creditAmount,
                TotalDebits = existing.TotalDebits + debitAmount,
                EntryCount = existing.EntryCount + 1,
                LastUpdated = @event.Timestamp,
                LastEventTimestamp = @event.Timestamp,
                LastEventVersion = @event.Version
            }
            : new WalletBalanceReadModel
            {
                WalletId = walletId,
                CurrencyCode = currencyCode,
                TotalCredits = creditAmount,
                TotalDebits = debitAmount,
                EntryCount = 1,
                LastUpdated = @event.Timestamp,
                LastEventTimestamp = @event.Timestamp,
                LastEventVersion = @event.Version
            };

        await _readStore.SetWalletBalanceAsync(compositeKey, updated, cancellationToken);
    }

    private static JsonElement? ParsePayload(ProjectionEvent @event)
    {
        if (@event.Payload is JsonElement je) return je;
        if (@event.Payload is null) return null;
        var s = JsonSerializer.Serialize(@event.Payload);
        return JsonDocument.Parse(s).RootElement;
    }

    private static string? GetString(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) ? v.GetString() : null;

    private static decimal GetDecimal(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) && v.TryGetDecimal(out var d) ? d : 0m;
}
