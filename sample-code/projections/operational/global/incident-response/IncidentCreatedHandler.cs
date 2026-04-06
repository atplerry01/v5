using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Operational.Incident;

public static class IncidentCreatedHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IncidentReadStore store, CancellationToken ct)
    {
        var (incidentId, type, severity, priority, source) = ExtractFields(@event);
        if (incidentId is null) return;

        var model = new IncidentReadModel
        {
            IncidentId = incidentId,
            IncidentType = type ?? "unknown",
            Severity = severity ?? "medium",
            Priority = priority ?? "P3",
            Status = "created",
            Source = source ?? "manual",
            CreatedAt = @event.Timestamp,
            UpdatedAt = @event.Timestamp
        };

        await store.SetAsync(incidentId, model, ct);

        var timeline = new IncidentTimelineReadModel
        {
            IncidentId = incidentId,
            Entries = [new TimelineEntry { Timestamp = @event.Timestamp, Action = "created" }]
        };
        await store.SetTimelineAsync(incidentId, timeline, ct);

        await UpdateMetrics(store, @event.Timestamp, m => m with
        {
            TotalIncidents = m.TotalIncidents + 1,
            OpenIncidents = m.OpenIncidents + 1,
            LastUpdated = @event.Timestamp
        }, ct);
    }

    private static (string? id, string? type, string? severity, string? priority, string? source) ExtractFields(ProjectionEvent e)
    {
        if (e.Payload is not JsonElement json)
        {
            var s = JsonSerializer.Serialize(e.Payload);
            json = JsonDocument.Parse(s).RootElement;
        }

        return (
            json.TryGetProperty("AggregateId", out var a) ? a.GetString() : null,
            json.TryGetProperty("IncidentType", out var t) ? t.GetString() : null,
            json.TryGetProperty("Severity", out var sv) ? sv.GetString() : null,
            json.TryGetProperty("Priority", out var p) ? p.GetString() : null,
            json.TryGetProperty("Source", out var src) ? src.GetString() : null
        );
    }

    internal static async Task UpdateMetrics(
        IncidentReadStore store,
        DateTimeOffset fallbackTimestamp,
        Func<IncidentMetricsReadModel, IncidentMetricsReadModel> update,
        CancellationToken ct)
    {
        var existing = await store.GetMetricsAsync(ct) ?? new IncidentMetricsReadModel { LastUpdated = fallbackTimestamp };
        await store.SetMetricsAsync(update(existing), ct);
    }
}
