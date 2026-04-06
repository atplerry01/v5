using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Operational.Incident;

public static class IncidentAssignedHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IncidentReadStore store, CancellationToken ct)
    {
        var (incidentId, assignee, level) = ExtractFields(@event);
        if (incidentId is null) return;

        var existing = await store.GetAsync(incidentId, ct);
        if (existing is null) return;

        await store.SetAsync(incidentId, existing with
        {
            Status = "assigned",
            AssignedTo = assignee,
            EscalationLevel = level,
            UpdatedAt = @event.Timestamp
        }, ct);

        await AppendTimeline(store, incidentId, @event.Timestamp, "assigned", assignee, ct);
    }

    private static (string? id, string? assignee, int level) ExtractFields(ProjectionEvent e)
    {
        if (e.Payload is not JsonElement json)
        {
            var s = JsonSerializer.Serialize(e.Payload);
            json = JsonDocument.Parse(s).RootElement;
        }

        return (
            json.TryGetProperty("AggregateId", out var a) ? a.GetString() : null,
            json.TryGetProperty("AssigneeIdentityId", out var ass) ? ass.GetString() : null,
            json.TryGetProperty("EscalationLevel", out var lv) ? lv.GetInt32() : 1
        );
    }

    internal static async Task AppendTimeline(
        IncidentReadStore store, string incidentId,
        DateTimeOffset timestamp, string action, string? actor, CancellationToken ct)
    {
        var timeline = await store.GetTimelineAsync(incidentId, ct);
        var entries = timeline?.Entries.ToList() ?? [];
        entries.Add(new TimelineEntry { Timestamp = timestamp, Action = action, Actor = actor });
        await store.SetTimelineAsync(incidentId, new IncidentTimelineReadModel
        {
            IncidentId = incidentId,
            Entries = entries.AsReadOnly()
        }, ct);
    }
}
