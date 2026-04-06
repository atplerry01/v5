using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Geo.RegionMapping;

public sealed class RegionMappingProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.geo.region-mapping";

    public string[] EventTypes =>
    [
        "whyce.intelligence.geo.region-mapping.created",
        "whyce.intelligence.geo.region-mapping.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRegionMappingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RegionMappingReadModel
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
