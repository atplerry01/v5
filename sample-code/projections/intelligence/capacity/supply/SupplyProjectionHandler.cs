using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Capacity.Supply;

public sealed class SupplyProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.capacity.supply";

    public string[] EventTypes =>
    [
        "whyce.intelligence.capacity.supply.created",
        "whyce.intelligence.capacity.supply.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISupplyViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SupplyReadModel
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
