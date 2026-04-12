namespace Whycespace.Domain.BusinessSystem.Logistic.Shipment;

public sealed record ShipmentCreatedEvent(
    ShipmentId ShipmentId,
    Origin Origin,
    Destination Destination,
    ItemReference ItemReference);
