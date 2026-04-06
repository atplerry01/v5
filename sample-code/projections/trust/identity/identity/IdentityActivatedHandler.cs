using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Identity;

public static class IdentityActivatedHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IdentityReadStore store, CancellationToken ct)
    {
        var json = IdentityRegisteredHandler.ParsePayload(@event);
        var identityId = json.GetStringOrNull("IdentityId");
        if (identityId is null) return;

        var existing = await store.GetIdentityAsync(identityId, ct);
        if (existing is null) return;

        // Idempotency: skip if already in target state
        if (existing.Status == "Active") return;

        await store.SetIdentityAsync(identityId, existing with
        {
            Status = "Active",
            ActivatedAt = @event.Timestamp
        }, ct);

        await IdentityRegisteredHandler.UpdateMetrics(store, @event.Timestamp, m => m with
        {
            ActiveIdentities = m.ActiveIdentities + 1,
            LastUpdated = @event.Timestamp
        }, ct);
    }
}
