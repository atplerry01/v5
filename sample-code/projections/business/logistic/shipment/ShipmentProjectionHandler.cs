using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Logistic.Shipment;

public sealed class ShipmentProjectionHandler
{
    public string ProjectionName => "whyce.business.logistic.shipment";

    public string[] EventTypes =>
    [
        "whyce.business.logistic.shipment.created",
        "whyce.business.logistic.shipment.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IShipmentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ShipmentReadModel
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
