using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Economic;

/// <summary>
/// Projects transaction history from all transaction.* events.
/// Tracks TransactionId, Status, Amount, Timestamp.
/// Global ordering: Timestamp + Version tiebreaker. Older events skipped.
/// </summary>
public sealed class TransactionHistoryProjectionHandler : IdempotentEconomicProjectionHandler
{
    private readonly EconomicReadStore _readStore;

    public TransactionHistoryProjectionHandler(EconomicReadStore readStore, IClock clock) : base(clock)
    {
        _readStore = readStore ?? throw new ArgumentNullException(nameof(readStore));
    }

    public override string ProjectionName => "economic.transaction-history";

    public override string[] EventTypes =>
    [
        "economic.transaction.initiated",
        "economic.transaction.approved",
        "economic.transaction.rejected",
        "economic.transaction.completed",
        "economic.transaction.settled"
    ];

    protected override async Task ApplyAsync(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken)
    {
        var json = ParsePayload(@event);
        if (json is null) return;

        var transactionId = GetString(json.Value, "TransactionId");
        if (transactionId is null) return;

        var status = DeriveStatus(@event.EventType);
        var existing = await _readStore.GetTransactionAsync(transactionId, cancellationToken);

        if (existing is not null)
        {
            // Global ordering guard
            if (ShouldSkipEvent(@event.Timestamp, @event.Version,
                    existing.LastEventTimestamp, existing.LastEventVersion))
                return;

            var updated = existing with
            {
                Status = status,
                Version = existing.Version + 1,
                UpdatedAt = @event.Timestamp,
                LastEventTimestamp = @event.Timestamp,
                LastEventVersion = @event.Version
            };

            await _readStore.SetTransactionAsync(transactionId, updated, cancellationToken);
        }
        else
        {
            var model = new TransactionHistoryReadModel
            {
                TransactionId = transactionId,
                Status = status,
                Amount = GetDecimal(json.Value, "Amount"),
                CurrencyCode = GetString(json.Value, "CurrencyCode") ?? "",
                SourceWalletId = GetString(json.Value, "SourceWalletId"),
                DestinationWalletId = GetString(json.Value, "DestinationWalletId"),
                Version = 1,
                CreatedAt = @event.Timestamp,
                UpdatedAt = @event.Timestamp,
                LastEventTimestamp = @event.Timestamp,
                LastEventVersion = @event.Version
            };

            await _readStore.SetTransactionAsync(transactionId, model, cancellationToken);
        }
    }

    private static string DeriveStatus(string eventType) => eventType switch
    {
        "economic.transaction.initiated" => "initiated",
        "economic.transaction.approved" => "approved",
        "economic.transaction.rejected" => "rejected",
        "economic.transaction.completed" => "completed",
        "economic.transaction.settled" => "settled",
        _ => "unknown"
    };

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
