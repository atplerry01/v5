using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Lot;

public sealed class LotProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.lot";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.lot.created",
        "whyce.business.inventory.lot.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ILotViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new LotReadModel
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
