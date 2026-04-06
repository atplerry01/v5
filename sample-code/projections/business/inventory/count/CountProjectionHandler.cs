using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Count;

public sealed class CountProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.count";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.count.created",
        "whyce.business.inventory.count.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICountViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CountReadModel
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
