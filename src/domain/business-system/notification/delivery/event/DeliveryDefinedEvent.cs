namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public sealed record DeliveryDefinedEvent(DeliveryId DeliveryId, DeliveryContract Contract);
