using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Inventory.Transfer;

public sealed class TransferProjectionHandler
{
    public string ProjectionName => "whyce.business.inventory.transfer";

    public string[] EventTypes =>
    [
        "whyce.business.inventory.transfer.created",
        "whyce.business.inventory.transfer.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITransferViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TransferReadModel
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
