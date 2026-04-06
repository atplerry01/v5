using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public sealed record ChannelDeactivatedEvent(Guid ChannelId) : DomainEvent;
