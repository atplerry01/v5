namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public readonly record struct ChannelId
{
    public Guid Value { get; }

    public ChannelId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ChannelId value must not be empty.", nameof(value));

        Value = value;
    }
}
