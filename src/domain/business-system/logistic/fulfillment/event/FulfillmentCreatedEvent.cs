namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed record FulfillmentCreatedEvent(
    FulfillmentId FulfillmentId,
    ShipmentReference ShipmentReference);
