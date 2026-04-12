namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public sealed record ChannelRegisteredEvent(ChannelId ChannelId, ChannelType ChannelType);
