using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Route;

public sealed class RouteProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.route";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.route.created",
        "whyce.orchestration.workflow.route.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRouteViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RouteReadModel
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
