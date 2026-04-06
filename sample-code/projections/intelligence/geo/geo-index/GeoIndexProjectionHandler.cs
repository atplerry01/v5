using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Geo.GeoIndex;

public sealed class GeoIndexProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.geo.geo-index";

    public string[] EventTypes =>
    [
        "whyce.intelligence.geo.geo-index.created",
        "whyce.intelligence.geo.geo-index.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IGeoIndexViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new GeoIndexReadModel
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
