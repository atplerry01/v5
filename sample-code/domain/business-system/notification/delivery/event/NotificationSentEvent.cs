using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public sealed record NotificationSentEvent(
    Guid DeliveryId,
    int AttemptCount
) : DomainEvent;
