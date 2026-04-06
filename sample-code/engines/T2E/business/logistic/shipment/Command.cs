namespace Whycespace.Engines.T2E.Business.Logistic.Shipment;

public record ShipmentCommand(
    string Action,
    string EntityId,
    object Payload
);
