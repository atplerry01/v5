using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Identity.Device;

public sealed class DeviceProjectionHandler
{
    public string ProjectionName => "whyce.trust.identity.device";

    public string[] EventTypes =>
    [
        "whyce.trust.identity.device.created",
        "whyce.trust.identity.device.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IDeviceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new DeviceReadModel
        {
            Id = @event.AggregateId.ToString(),
            Status = "Active",
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model, ct);
    }
}
