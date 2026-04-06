using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Geo.Proximity;

public sealed class ProximityProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.geo.proximity";

    public string[] EventTypes =>
    [
        "whyce.intelligence.geo.proximity.created",
        "whyce.intelligence.geo.proximity.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IProximityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ProximityReadModel
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
