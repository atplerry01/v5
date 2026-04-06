using System.Text.Json;
using Whycespace.Projections.Economic;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Chain.Handlers;

/// <summary>
/// Projects chain state from whyce.chain.block.created events.
/// Maintains: current head, block height, last timestamp.
/// Idempotent via EventId deduplication (base class).
/// </summary>
public sealed class ChainStateHandler : IdempotentEconomicProjectionHandler
{
    private readonly IProjectionStore _store;

    public ChainStateHandler(IProjectionStore store, IClock clock) : base(clock)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public override string ProjectionName => "chain.state";
    public override string[] EventTypes => ["whyce.chain.block.created"];

    protected override async Task ApplyAsync(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken)
    {
        var json = ParsePayload(@event);
        if (json is null) return;

        var blockId = GetString(json.Value, "BlockId");
        var hash = GetString(json.Value, "CurrentHash");
        var sequenceNumber = GetLong(json.Value, "SequenceNumber");
        var timestamp = GetDateTimeOffset(json.Value, "Timestamp");

        if (blockId is null || hash is null) return;

        var existing = await store.GetAsync<ChainStateReadModel>(
            ProjectionName, ChainStateReadModel.Key, cancellationToken);

        if (existing is not null &&
            ShouldSkipEvent(@event.Timestamp, @event.Version,
                existing.LastUpdated, existing.LastEventVersion))
            return;

        var updated = new ChainStateReadModel
        {
            CurrentHeadHash = hash,
            CurrentHeadBlockId = blockId,
            BlockHeight = sequenceNumber,
            LastBlockTimestamp = timestamp ?? @event.Timestamp,
            LastUpdated = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await store.SetAsync(ProjectionName, ChainStateReadModel.Key, updated, cancellationToken);
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

    private static long GetLong(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) && v.TryGetInt64(out var l) ? l : 0;

    private static DateTimeOffset? GetDateTimeOffset(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) && v.TryGetDateTimeOffset(out var d) ? d : null;
}
