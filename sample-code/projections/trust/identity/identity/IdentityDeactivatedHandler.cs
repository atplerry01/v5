using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Identity;

public static class IdentityDeactivatedHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IdentityReadStore store, CancellationToken ct)
    {
        var json = IdentityRegisteredHandler.ParsePayload(@event);
        var identityId = json.GetStringOrNull("IdentityId");
        if (identityId is null) return;

        var existing = await store.GetIdentityAsync(identityId, ct);
        if (existing is null) return;

        // Idempotency: skip if already deactivated
        if (existing.Status == "Deactivated") return;

        var wasActive = existing.Status == "Active";
        var wasSuspended = existing.Status == "Suspended";

        await store.SetIdentityAsync(identityId, existing with
        {
            Status = "Deactivated",
            DeactivatedAt = @event.Timestamp
        }, ct);

        await IdentityRegisteredHandler.UpdateMetrics(store, @event.Timestamp, m => m with
        {
            ActiveIdentities = wasActive ? m.ActiveIdentities - 1 : m.ActiveIdentities,
            SuspendedIdentities = wasSuspended ? m.SuspendedIdentities - 1 : m.SuspendedIdentities,
            DeactivatedIdentities = m.DeactivatedIdentities + 1,
            LastUpdated = @event.Timestamp
        }, ct);
    }
}
