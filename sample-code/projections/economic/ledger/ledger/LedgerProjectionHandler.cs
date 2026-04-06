using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Economic.Handlers;

/// <summary>
/// Projects ledger ↔ policy decision links from anchored events that carry economic data.
/// Indexes by decisionHash and accountId for audit queries.
/// Idempotent via EventId deduplication (base class).
/// </summary>
public sealed class LedgerPolicyLinkProjectionHandler : IdempotentEconomicProjectionHandler
{
    public LedgerPolicyLinkProjectionHandler(IClock clock) : base(clock) { }

    public override string ProjectionName => "economic.ledger-policy-link";

    public override string[] EventTypes =>
    [
        "whyce.observability.policy.decision.anchored"
    ];

    protected override async Task ApplyAsync(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken)
    {
        var json = ParsePayload(@event);
        if (json is null) return;

        // Only process events with economic context
        var accountId = GetString(json.Value, "AccountId");
        if (string.IsNullOrWhiteSpace(accountId)) return;

        var decisionHash = GetString(json.Value, "DecisionHash");
        if (decisionHash is null) return;

        var model = new LedgerPolicyLinkReadModel
        {
            DecisionHash = decisionHash,
            AccountId = accountId,
            AssetId = GetString(json.Value, "AssetId") ?? "",
            Amount = GetDecimal(json.Value, "Amount"),
            Currency = GetString(json.Value, "Currency") ?? "USD",
            TransactionType = GetString(json.Value, "TransactionType") ?? "unknown",
            PolicyId = GetString(json.Value, "PolicyId") ?? "",
            Decision = GetString(json.Value, "Decision") ?? "",
            SubjectId = GetString(json.Value, "SubjectId") ?? "",
            BlockId = GetString(json.Value, "BlockId"),
            BlockHash = GetString(json.Value, "BlockHash"),
            AnchoredAt = @event.Timestamp,
            LastUpdated = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        var key = LedgerPolicyLinkReadModel.KeyFor(decisionHash);
        await store.SetAsync(ProjectionName, key, model, cancellationToken);

        // Index by accountId
        var indexKey = LedgerPolicyLinkIndexReadModel.KeyFor(accountId);
        var index = await store.GetAsync<LedgerPolicyLinkIndexReadModel>(
            ProjectionName, indexKey, cancellationToken);

        if (index is null)
        {
            index = new LedgerPolicyLinkIndexReadModel
            {
                AccountId = accountId,
                DecisionHashes = [decisionHash],
                LinkCount = 1,
                LastUpdated = @event.Timestamp
            };
        }
        else if (!index.DecisionHashes.Contains(decisionHash))
        {
            index = index with
            {
                DecisionHashes = [.. index.DecisionHashes, decisionHash],
                LinkCount = index.LinkCount + 1,
                LastUpdated = @event.Timestamp
            };
        }

        await store.SetAsync(ProjectionName, indexKey, index, cancellationToken);
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
