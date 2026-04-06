using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Identity;

public static class SessionRevokedHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IdentityReadStore store, CancellationToken ct)
    {
        var json = IdentityRegisteredHandler.ParsePayload(@event);
        var sessionId = json.GetStringOrNull("SessionId");
        if (sessionId is null) return;

        var existing = await store.GetSessionAsync(sessionId, ct);
        if (existing is null) return;

        // Idempotency: skip if already revoked
        if (existing.Status == "Revoked") return;

        await store.SetSessionAsync(sessionId, existing with { Status = "Revoked" }, ct);

        await IdentityRegisteredHandler.UpdateMetrics(store, @event.Timestamp, m => m with
        {
            ActiveSessions = m.ActiveSessions - 1,
            LastUpdated = @event.Timestamp
        }, ct);
    }
}
