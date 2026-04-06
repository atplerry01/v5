using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Logistic.Fulfillment;

public sealed class FulfillmentProjectionHandler
{
    public string ProjectionName => "whyce.business.logistic.fulfillment";

    public string[] EventTypes =>
    [
        "whyce.business.logistic.fulfillment.created",
        "whyce.business.logistic.fulfillment.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IFulfillmentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new FulfillmentReadModel
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
