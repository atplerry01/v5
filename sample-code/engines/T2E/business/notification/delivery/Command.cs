namespace Whycespace.Engines.T2E.Business.Notification.Delivery;

public record DeliveryCommand(
    string Action,
    string EntityId,
    object Payload
);
