using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Identity;

public static class IdentitySuspendedHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IdentityReadStore store, CancellationToken ct)
    {
        var json = IdentityRegisteredHandler.ParsePayload(@event);
        var identityId = json.GetStringOrNull("IdentityId");
        if (identityId is null) return;

        var existing = await store.GetIdentityAsync(identityId, ct);
        if (existing is null) return;

        // Idempotency: skip if already suspended
        if (existing.Status == "Suspended") return;

        // Only decrement active count if transitioning FROM Active
        var wasActive = existing.Status == "Active";

        await store.SetIdentityAsync(identityId, existing with { Status = "Suspended" }, ct);

        await IdentityRegisteredHandler.UpdateMetrics(store, @event.Timestamp, m => m with
        {
            ActiveIdentities = wasActive ? m.ActiveIdentities - 1 : m.ActiveIdentities,
            SuspendedIdentities = m.SuspendedIdentities + 1,
            LastUpdated = @event.Timestamp
        }, ct);
    }
}
