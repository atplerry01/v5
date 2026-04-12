namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public readonly record struct ChannelType
{
    public string Value { get; }

    public ChannelType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Channel type must not be empty.", nameof(value));

        Value = value;
    }
}
