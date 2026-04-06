using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public sealed record ChannelCreatedEvent(
    Guid ChannelId,
    string ChannelType
) : DomainEvent;
