using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Warehouse;

public sealed class WarehouseProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.warehouse";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.warehouse.created",
        "whyce.business.inventory.warehouse.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IWarehouseViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new WarehouseReadModel
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
