using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Receipt;

public sealed class ReceiptProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.receipt";

    public string[] EventTypes =>
    [
        "whyce.business.integration.receipt.created",
        "whyce.business.integration.receipt.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReceiptViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReceiptReadModel
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
