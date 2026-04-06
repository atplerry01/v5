using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Identity;

public static class DeviceRegisteredHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IdentityReadStore store, CancellationToken ct)
    {
        var json = IdentityRegisteredHandler.ParsePayload(@event);
        var deviceId = json.GetStringOrNull("DeviceId");
        var identityId = json.GetStringOrNull("IdentityId");
        if (deviceId is null || identityId is null) return;

        // Idempotency: skip if device already projected
        var existing = await store.GetDeviceAsync(deviceId, ct);
        if (existing is not null) return;

        var model = new IdentityDeviceReadModel
        {
            DeviceId = deviceId,
            IdentityId = identityId,
            DeviceType = json.GetStringOrNull("DeviceType") ?? "",
            Status = "Registered",
            RegisteredAt = @event.Timestamp,
            LastSeenAt = @event.Timestamp
        };

        await store.SetDeviceAsync(deviceId, model, ct);

        await IdentityRegisteredHandler.UpdateMetrics(store, @event.Timestamp, m => m with
        {
            TotalDevices = m.TotalDevices + 1,
            LastUpdated = @event.Timestamp
        }, ct);
    }
}
