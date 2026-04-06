using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Estimation.DemandSupply;

public sealed class DemandSupplyProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.estimation.demand-supply";

    public string[] EventTypes =>
    [
        "whyce.intelligence.estimation.demand-supply.created",
        "whyce.intelligence.estimation.demand-supply.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IDemandSupplyViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new DemandSupplyReadModel
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
