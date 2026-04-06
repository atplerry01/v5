using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Identity;

public static class SessionStartedHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IdentityReadStore store, CancellationToken ct)
    {
        var json = IdentityRegisteredHandler.ParsePayload(@event);
        var sessionId = json.GetStringOrNull("SessionId");
        var identityId = json.GetStringOrNull("IdentityId");
        if (sessionId is null || identityId is null) return;

        // Idempotency: skip if session already exists
        var existing = await store.GetSessionAsync(sessionId, ct);
        if (existing is not null) return;

        var model = new IdentitySessionReadModel
        {
            SessionId = sessionId,
            IdentityId = identityId,
            DeviceId = json.GetStringOrNull("DeviceId") ?? "",
            Status = "Active",
            CreatedAt = @event.Timestamp,
            ExpiresAt = @event.Timestamp.AddHours(24),
            LastAccessedAt = @event.Timestamp
        };

        await store.SetSessionAsync(sessionId, model, ct);

        await IdentityRegisteredHandler.UpdateMetrics(store, @event.Timestamp, m => m with
        {
            ActiveSessions = m.ActiveSessions + 1,
            LastUpdated = @event.Timestamp
        }, ct);
    }
}
