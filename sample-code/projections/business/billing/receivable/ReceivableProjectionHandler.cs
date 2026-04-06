using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Billing.Receivable;

public sealed class ReceivableProjectionHandler
{
    public string ProjectionName => "whyce.business.billing.receivable";

    public string[] EventTypes =>
    [
        "whyce.business.billing.receivable.created",
        "whyce.business.billing.receivable.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReceivableViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReceivableReadModel
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
