using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Operational.Incident;

public static class IncidentEscalatedHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IncidentReadStore store, CancellationToken ct)
    {
        var (incidentId, newSeverity) = ExtractFields(@event);
        if (incidentId is null) return;

        var existing = await store.GetAsync(incidentId, ct);
        if (existing is null) return;

        await store.SetAsync(incidentId, existing with
        {
            Status = "escalated",
            Severity = newSeverity ?? existing.Severity,
            EscalationLevel = existing.EscalationLevel + 1,
            SLAStatus = "breached",
            UpdatedAt = @event.Timestamp
        }, ct);

        await IncidentAssignedHandler.AppendTimeline(
            store, incidentId, @event.Timestamp, "escalated", null, ct);

        await IncidentCreatedHandler.UpdateMetrics(store, @event.Timestamp, m => m with
        {
            EscalatedIncidents = m.EscalatedIncidents + 1,
            SLABreachCount = m.SLABreachCount + 1,
            LastUpdated = @event.Timestamp
        }, ct);
    }

    private static (string? id, string? newSeverity) ExtractFields(ProjectionEvent e)
    {
        if (e.Payload is not JsonElement json)
        {
            var s = JsonSerializer.Serialize(e.Payload);
            json = JsonDocument.Parse(s).RootElement;
        }

        return (
            json.TryGetProperty("AggregateId", out var a) ? a.GetString() : null,
            json.TryGetProperty("NewSeverity", out var ns) ? ns.GetString() : null
        );
    }
}
