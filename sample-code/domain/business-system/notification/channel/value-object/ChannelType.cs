namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public sealed record ChannelType(string Value)
{
    public static readonly ChannelType Email = new("email");
    public static readonly ChannelType SMS = new("sms");
    public static readonly ChannelType Push = new("push");
    public static readonly ChannelType Webhook = new("webhook");
}
