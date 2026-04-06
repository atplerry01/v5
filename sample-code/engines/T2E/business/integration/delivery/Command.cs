namespace Whycespace.Engines.T2E.Business.Integration.Delivery;

public record DeliveryCommand(
    string Action,
    string EntityId,
    object Payload
);
