using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Adjustment;

public sealed class AdjustmentProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.adjustment";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.adjustment.created",
        "whyce.business.inventory.adjustment.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAdjustmentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AdjustmentReadModel
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
