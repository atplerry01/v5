using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Delivery;

public sealed class DeliveryProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.delivery";

    public string[] EventTypes =>
    [
        "whyce.business.integration.delivery.created",
        "whyce.business.integration.delivery.updated"
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
