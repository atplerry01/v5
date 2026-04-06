using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public sealed record NotificationRetriedEvent(
    Guid DeliveryId,
    int AttemptCount
) : DomainEvent;
