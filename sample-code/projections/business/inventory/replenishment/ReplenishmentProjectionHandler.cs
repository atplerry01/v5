using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Replenishment;

public sealed class ReplenishmentProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.replenishment";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.replenishment.created",
        "whyce.business.inventory.replenishment.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReplenishmentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReplenishmentReadModel
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
