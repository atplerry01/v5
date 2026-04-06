using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Geo.Geofence;

public sealed class GeofenceProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.geo.geofence";

    public string[] EventTypes =>
    [
        "whyce.intelligence.geo.geofence.created",
        "whyce.intelligence.geo.geofence.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IGeofenceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new GeofenceReadModel
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
