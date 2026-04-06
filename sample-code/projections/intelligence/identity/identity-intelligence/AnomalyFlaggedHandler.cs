using System.Text.Json;
using Whycespace.Projections.IdentityIntelligence.ReadModels;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.IdentityIntelligence.Handlers;

/// <summary>
/// Projection handler for anomaly.flagged events.
/// Appends to existing anomaly list (idempotent via event timestamp check).
/// </summary>
public static class AnomalyFlaggedHandler
{
    public static async Task HandleAsync(
        ProjectionEvent @event,
        Queries.IntelligenceReadStore store,
        CancellationToken ct)
    {
        if (@event.Payload is null) return;

        var json = JsonSerializer.Serialize(@event.Payload);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var identityId = root.GetProperty("IdentityId").GetString()!;
        var anomalyType = root.GetProperty("AnomalyType").GetString()!;
        var description = root.GetProperty("Description").GetString()!;
        var confidence = root.GetProperty("Confidence").GetDecimal();

        var existing = await store.GetAnomaliesAsync(identityId, ct);
        var anomalies = existing?.Anomalies?.ToList() ?? [];

        anomalies.Add(new AnomalyFlagReadModel
        {
            AnomalyType = anomalyType,
            Description = description,
            Confidence = confidence,
            DetectedAt = @event.Timestamp
        });

        var model = new AnomalyReadModel
        {
            IdentityId = identityId,
            HasActiveAnomalies = anomalies.Count > 0,
            Anomalies = anomalies,
            LastCheckedAt = @event.Timestamp
        };

        await store.SetAnomaliesAsync(identityId, model, ct);
    }
}
