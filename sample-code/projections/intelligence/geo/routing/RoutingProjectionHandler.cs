using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Geo.Routing;

public sealed class RoutingProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.geo.routing";

    public string[] EventTypes =>
    [
        "whyce.intelligence.geo.routing.created",
        "whyce.intelligence.geo.routing.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRoutingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RoutingReadModel
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
