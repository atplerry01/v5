namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public static class ChannelErrors
{
    public static ChannelDomainException MissingId()
        => new("ChannelId is required and must not be empty.");

    public static ChannelDomainException InvalidChannelType()
        => new("Channel must define a valid type.");

    public static ChannelDomainException InvalidStateTransition(ChannelStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class ChannelDomainException : Exception
{
    public ChannelDomainException(string message) : base(message) { }
}
