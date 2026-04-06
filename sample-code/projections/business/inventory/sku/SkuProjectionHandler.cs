using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Sku;

public sealed class SkuProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.sku";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.sku.created",
        "whyce.business.inventory.sku.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISkuViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SkuReadModel
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
