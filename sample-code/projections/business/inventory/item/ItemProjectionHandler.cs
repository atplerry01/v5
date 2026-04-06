using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Item;

public sealed class ItemProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.item";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.item.created",
        "whyce.business.inventory.item.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IItemViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ItemReadModel
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
