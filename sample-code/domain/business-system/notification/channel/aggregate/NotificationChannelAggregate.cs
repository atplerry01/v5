using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public sealed class NotificationChannelAggregate : AggregateRoot
{
    public ChannelId ChannelId { get; private set; }
    public ChannelType ChannelType { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static NotificationChannelAggregate Create(Guid channelId, ChannelType channelType)
    {
        var channel = new NotificationChannelAggregate();
        channel.Apply(new ChannelCreatedEvent(channelId, channelType.Value));
        return channel;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(ChannelErrors.AlreadyDeactivated, "Channel is already deactivated.");

        Apply(new ChannelDeactivatedEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException(ChannelErrors.AlreadyActive, "Channel is already active.");

        Apply(new ChannelActivatedEvent(Id));
    }

    private void Apply(ChannelCreatedEvent e)
    {
        Id = e.ChannelId;
        ChannelId = new ChannelId(e.ChannelId);
        ChannelType = new ChannelType(e.ChannelType);
        IsActive = true;
        CreatedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }

    private void Apply(ChannelDeactivatedEvent e)
    {
        IsActive = false;
        RaiseDomainEvent(e);
    }

    private void Apply(ChannelActivatedEvent e)
    {
        IsActive = true;
        RaiseDomainEvent(e);
    }
}
