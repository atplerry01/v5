using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public sealed record NotificationFailedEvent(
    Guid DeliveryId,
    int AttemptCount,
    string Reason
) : DomainEvent;
