using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Stock;

public sealed class StockProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.stock";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.stock.created",
        "whyce.business.inventory.stock.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IStockViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new StockReadModel
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
