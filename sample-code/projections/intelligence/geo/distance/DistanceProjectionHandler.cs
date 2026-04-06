using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Geo.Distance;

public sealed class DistanceProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.geo.distance";

    public string[] EventTypes =>
    [
        "whyce.intelligence.geo.distance.created",
        "whyce.intelligence.geo.distance.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IDistanceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new DistanceReadModel
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
