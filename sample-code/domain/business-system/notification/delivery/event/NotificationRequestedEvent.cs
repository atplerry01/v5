using Whycespace.Domain.BusinessSystem.Notification.Channel;
using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public sealed record NotificationRequestedEvent(
    Guid DeliveryId,
    ChannelId ChannelId
) : DomainEvent;
