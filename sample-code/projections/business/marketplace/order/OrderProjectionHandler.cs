using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Marketplace.Order;

public sealed class OrderProjectionHandler
{
    public string ProjectionName => "whyce.business.marketplace.order";

    public string[] EventTypes =>
    [
        "whyce.business.marketplace.order.created",
        "whyce.business.marketplace.order.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IOrderViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new OrderReadModel
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
