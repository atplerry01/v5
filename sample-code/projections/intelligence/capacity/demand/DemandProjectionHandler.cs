using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Capacity.Demand;

public sealed class DemandProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.capacity.demand";

    public string[] EventTypes =>
    [
        "whyce.intelligence.capacity.demand.created",
        "whyce.intelligence.capacity.demand.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IDemandViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new DemandReadModel
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
