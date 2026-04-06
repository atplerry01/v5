using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Batch;

public sealed class BatchProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.batch";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.batch.created",
        "whyce.business.inventory.batch.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IBatchViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new BatchReadModel
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
