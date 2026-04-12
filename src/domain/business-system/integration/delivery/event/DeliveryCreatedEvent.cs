namespace Whycespace.Domain.BusinessSystem.Integration.Delivery;

public sealed record DeliveryScheduledEvent(DeliveryId DeliveryId, DeliveryDescriptor Descriptor);

public sealed record DeliveryDispatchedEvent(DeliveryId DeliveryId);

public sealed record DeliveryConfirmedEvent(DeliveryId DeliveryId);

public sealed record DeliveryFailedEvent(DeliveryId DeliveryId);
