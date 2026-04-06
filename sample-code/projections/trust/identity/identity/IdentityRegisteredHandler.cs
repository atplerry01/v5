using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Identity;

public static class IdentityRegisteredHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IdentityReadStore store, CancellationToken ct)
    {
        var json = ParsePayload(@event);
        var identityId = json.GetStringOrNull("IdentityId") ?? json.GetStringOrNull("AggregateId");
        if (identityId is null) return;

        // Idempotency: skip if identity already projected
        var existing = await store.GetIdentityAsync(identityId, ct);
        if (existing is not null) return;

        var model = new IdentityReadModel
        {
            IdentityId = identityId,
            IdentityType = json.GetStringOrNull("IdentityType") ?? "unknown",
            DisplayName = json.GetStringOrNull("DisplayName") ?? "",
            Status = "Pending",
            CreatedAt = @event.Timestamp
        };

        await store.SetIdentityAsync(identityId, model, ct);
        await UpdateMetrics(store, @event.Timestamp, m => m with { TotalIdentities = m.TotalIdentities + 1, LastUpdated = @event.Timestamp }, ct);
    }

    internal static JsonElement ParsePayload(ProjectionEvent e)
    {
        if (e.Payload is JsonElement json) return json;
        var s = JsonSerializer.Serialize(e.Payload);
        return JsonDocument.Parse(s).RootElement;
    }

    internal static string? GetStringOrNull(this JsonElement json, string property)
        => json.TryGetProperty(property, out var prop) && prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;

    internal static async Task UpdateMetrics(IdentityReadStore store, DateTimeOffset fallbackTimestamp, Func<IdentityMetricsReadModel, IdentityMetricsReadModel> update, CancellationToken ct)
    {
        var existing = await store.GetMetricsAsync(ct) ?? new IdentityMetricsReadModel { LastUpdated = fallbackTimestamp };
        await store.SetMetricsAsync(update(existing), ct);
    }
}
