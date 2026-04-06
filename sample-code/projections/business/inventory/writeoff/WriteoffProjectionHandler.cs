using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Writeoff;

public sealed class WriteoffProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.writeoff";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.writeoff.created",
        "whyce.business.inventory.writeoff.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IWriteoffViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new WriteoffReadModel
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
