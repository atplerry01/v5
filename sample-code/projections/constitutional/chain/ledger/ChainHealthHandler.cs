using System.Text.Json;
using Whycespace.Projections.Economic;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Contracts.Infrastructure.Storage;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Chain.Handlers;

/// <summary>
/// Projects chain health from whyce.chain.block.created events.
/// Tracks: continuity validity, hash consistency, missing sequence detection.
/// Validates each new block against expected chain state.
/// Idempotent via EventId deduplication (base class).
/// </summary>
public sealed class ChainHealthHandler : IdempotentEconomicProjectionHandler
{
    public ChainHealthHandler(IClock clock) : base(clock) { }

    public override string ProjectionName => "chain.health";
    public override string[] EventTypes => ["whyce.chain.block.created"];

    protected override async Task ApplyAsync(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken)
    {
        var json = ParsePayload(@event);
        if (json is null) return;

        var blockId = GetString(json.Value, "BlockId");
        var currentHash = GetString(json.Value, "CurrentHash");
        var previousHash = GetString(json.Value, "PreviousHash");
        var sequenceNumber = GetLong(json.Value, "SequenceNumber");

        if (blockId is null || currentHash is null || previousHash is null) return;

        var existing = await store.GetAsync<ChainHealthReadModel>(
            ProjectionName, ChainHealthReadModel.Key, cancellationToken);

        if (existing is not null &&
            ShouldSkipEvent(@event.Timestamp, @event.Version,
                existing.LastChecked, existing.LastEventVersion))
            return;

        // Check continuity: does this block's previous_hash match what we expect?
        var expectedPrevHash = existing?.ExpectedPreviousHash ?? ChainBlock.GenesisHash;
        var expectedSeq = existing?.ExpectedNextSequence ?? 1;

        var continuityValid = previousHash == expectedPrevHash;
        var sequenceValid = sequenceNumber == expectedSeq;
        var hashConsistent = (existing?.HashConsistent ?? true) && continuityValid;

        var missingSequences = existing?.MissingSequences ?? [];
        var missingCount = existing?.MissingSequenceCount ?? 0;

        // Detect sequence gaps
        if (sequenceNumber > expectedSeq)
        {
            var gap = Enumerable.Range((int)expectedSeq, (int)(sequenceNumber - expectedSeq))
                .Select(s => (long)s)
                .ToList();
            missingSequences = [.. missingSequences, .. gap];
            missingCount += gap.Count;
        }

        var updated = new ChainHealthReadModel
        {
            ContinuityValid = continuityValid && sequenceValid,
            HashConsistent = hashConsistent,
            ExpectedNextSequence = sequenceNumber + 1,
            ExpectedPreviousHash = currentHash,
            MissingSequenceCount = missingCount,
            MissingSequences = missingSequences,
            ContinuityBreakCount = (existing?.ContinuityBreakCount ?? 0) + (continuityValid ? 0 : 1),
            LastChecked = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await store.SetAsync(ProjectionName, ChainHealthReadModel.Key, updated, cancellationToken);
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
}
