using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Notification.Delivery;

public sealed class DeliveryProjectionHandler
{
    public string ProjectionName => "whyce.business.notification.delivery";

    public string[] EventTypes =>
    [
        "whyce.business.notification.delivery.created",
        "whyce.business.notification.delivery.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IDeliveryViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new DeliveryReadModel
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
