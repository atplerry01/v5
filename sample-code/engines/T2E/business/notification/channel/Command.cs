namespace Whycespace.Engines.T2E.Business.Notification.Channel;

public record ChannelCommand(
    string Action,
    string EntityId,
    object Payload
);
