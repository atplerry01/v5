using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Operational.Incident;

public static class IncidentClosedHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IncidentReadStore store, CancellationToken ct)
    {
        var incidentId = ExtractId(@event);
        if (incidentId is null) return;

        var existing = await store.GetAsync(incidentId, ct);
        if (existing is null) return;

        await store.SetAsync(incidentId, existing with
        {
            Status = "closed",
            ClosedAt = @event.Timestamp,
            UpdatedAt = @event.Timestamp
        }, ct);

        await IncidentAssignedHandler.AppendTimeline(
            store, incidentId, @event.Timestamp, "closed", null, ct);

        await IncidentCreatedHandler.UpdateMetrics(store, @event.Timestamp, m => m with
        {
            ClosedIncidents = m.ClosedIncidents + 1,
            LastUpdated = @event.Timestamp
        }, ct);
    }

    private static string? ExtractId(ProjectionEvent e)
    {
        if (e.Payload is not JsonElement json)
        {
            var s = JsonSerializer.Serialize(e.Payload);
            json = JsonDocument.Parse(s).RootElement;
        }

        return json.TryGetProperty("AggregateId", out var a) ? a.GetString() : null;
    }
}
