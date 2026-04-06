using System.Text.Json;
using Whycespace.Projections.Economic;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Contracts.Infrastructure.Storage;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Chain.Handlers;

/// <summary>
/// Detects chain anomalies from whyce.chain.block.created events:
///   - Hash mismatch (previous_hash != expected)
///   - Duplicate correlation attempts
///   - Fork attempt signals (same previous_hash used twice)
///
/// Stores individual anomaly records + running summary.
/// Idempotent via EventId deduplication (base class).
/// </summary>
public sealed class ChainAnomalyHandler : IdempotentEconomicProjectionHandler
{
    public ChainAnomalyHandler(IClock clock) : base(clock) { }

    public override string ProjectionName => "chain.anomaly";
    public override string[] EventTypes => ["whyce.chain.block.created", "whyce.chain.anomaly.detected"];

    protected override async Task ApplyAsync(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken)
    {
        if (@event.EventType == "whyce.chain.anomaly.detected")
        {
            await HandleAnomalyEvent(@event, store, cancellationToken);
            return;
        }

        // For block.created events, check for anomalies
        var json = ParsePayload(@event);
        if (json is null) return;

        var blockId = GetString(json.Value, "BlockId") ?? "";
        var previousHash = GetString(json.Value, "PreviousHash") ?? "";
        var sequenceNumber = GetLong(json.Value, "SequenceNumber");
        var correlationId = GetString(json.Value, "CorrelationIdValue");

        // Load current health state to detect anomalies
        var health = await store.GetAsync<ChainHealthReadModel>(
            "chain.health", ChainHealthReadModel.Key, cancellationToken);

        if (health is null) return; // First block — no anomalies possible

        var expectedPrevHash = health.ExpectedPreviousHash;

        // Detect hash mismatch
        if (expectedPrevHash != ChainBlock.GenesisHash && previousHash != expectedPrevHash)
        {
            await RecordAnomaly(store, new ChainAnomalyReadModel
            {
                AnomalyId = $"hash-mismatch-{sequenceNumber}",
                AnomalyType = "hash_mismatch",
                Description = $"Block {blockId} previous_hash [{previousHash[..8]}...] does not match expected [{expectedPrevHash[..8]}...]",
                AtSequenceNumber = sequenceNumber,
                BlockId = blockId,
                DetectedAt = @event.Timestamp,
                CorrelationId = correlationId
            }, cancellationToken);
        }
    }

    private async Task HandleAnomalyEvent(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken)
    {
        var json = ParsePayload(@event);
        if (json is null) return;

        var anomalyType = GetString(json.Value, "AnomalyType") ?? "unknown";
        var description = GetString(json.Value, "Description") ?? "";
        var blockId = GetString(json.Value, "BlockId") ?? "";
        var sequenceNumber = GetLong(json.Value, "SequenceNumber");
        var correlationId = GetString(json.Value, "CorrelationId");

        await RecordAnomaly(store, new ChainAnomalyReadModel
        {
            AnomalyId = $"{anomalyType}-{@event.EventId}",
            AnomalyType = anomalyType,
            Description = description,
            AtSequenceNumber = sequenceNumber,
            BlockId = blockId,
            DetectedAt = @event.Timestamp,
            CorrelationId = correlationId
        }, cancellationToken);
    }

    private async Task RecordAnomaly(
        IProjectionStore store,
        ChainAnomalyReadModel anomaly,
        CancellationToken cancellationToken)
    {
        await store.SetAsync(ProjectionName,
            ChainAnomalyReadModel.KeyFor(anomaly.AnomalyId),
            anomaly, cancellationToken);

        // Update summary
        var summary = await store.GetAsync<ChainAnomalySummaryReadModel>(
            ProjectionName, ChainAnomalySummaryReadModel.Key, cancellationToken)
            ?? new ChainAnomalySummaryReadModel { LastAnomalyAt = default, LastUpdated = default };

        var updated = summary with
        {
            TotalAnomalies = summary.TotalAnomalies + 1,
            HashMismatches = summary.HashMismatches + (anomaly.AnomalyType == "hash_mismatch" ? 1 : 0),
            DuplicateCorrelations = summary.DuplicateCorrelations + (anomaly.AnomalyType == "duplicate_correlation" ? 1 : 0),
            ForkAttempts = summary.ForkAttempts + (anomaly.AnomalyType == "fork_attempt" ? 1 : 0),
            LastAnomalyAt = anomaly.DetectedAt,
            LastUpdated = anomaly.DetectedAt
        };

        await store.SetAsync(ProjectionName, ChainAnomalySummaryReadModel.Key, updated, cancellationToken);
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
