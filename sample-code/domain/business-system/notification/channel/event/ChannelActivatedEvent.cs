using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public sealed record ChannelActivatedEvent(Guid ChannelId) : DomainEvent;
