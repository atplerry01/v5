using Whycespace.Domain.BusinessSystem.Notification.Channel;
using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public sealed class NotificationDeliveryAggregate : AggregateRoot
{
    private const int MaxRetries = 3;

    public DeliveryId DeliveryId { get; private set; }
    public ChannelId ChannelId { get; private set; }
    public DeliveryStatus Status { get; private set; } = DeliveryStatus.Pending;
    public int AttemptCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static NotificationDeliveryAggregate Request(Guid deliveryId, ChannelId channelId)
    {
        var delivery = new NotificationDeliveryAggregate();
        delivery.Apply(new NotificationRequestedEvent(deliveryId, channelId));
        return delivery;
    }

    public void Send()
    {
        if (Status == DeliveryStatus.Sent)
            throw new DomainException(DeliveryErrors.AlreadySent, "Notification has already been sent.");

        if (Status.IsTerminal)
            throw new DomainException(DeliveryErrors.InvalidTransition, $"Cannot send notification in '{Status.Value}' status.");

        Apply(new NotificationSentEvent(Id, AttemptCount + 1));
    }

    public void Retry()
    {
        if (Status.IsTerminal && Status == DeliveryStatus.Sent)
            throw new DomainException(DeliveryErrors.AlreadySent, "Cannot retry a sent notification.");

        if (AttemptCount >= MaxRetries)
            throw new DomainException(DeliveryErrors.MaxRetriesExceeded, $"Max retries ({MaxRetries}) exceeded.");

        Apply(new NotificationRetriedEvent(Id, AttemptCount + 1));
    }

    public void MarkFailed(string reason)
    {
        if (Status == DeliveryStatus.Failed)
            throw new DomainException(DeliveryErrors.AlreadyFailed, "Notification is already marked as failed.");

        if (Status == DeliveryStatus.Sent)
            throw new DomainException(DeliveryErrors.InvalidTransition, "Cannot fail a sent notification.");

        Apply(new NotificationFailedEvent(Id, AttemptCount, reason));
    }

    private void Apply(NotificationRequestedEvent e)
    {
        Id = e.DeliveryId;
        DeliveryId = new DeliveryId(e.DeliveryId);
        ChannelId = e.ChannelId;
        Status = DeliveryStatus.Pending;
        AttemptCount = 0;
        CreatedAt = e.OccurredAt;
        UpdatedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }

    private void Apply(NotificationSentEvent e)
    {
        Status = DeliveryStatus.Sent;
        AttemptCount = e.AttemptCount;
        UpdatedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }

    private void Apply(NotificationRetriedEvent e)
    {
        AttemptCount = e.AttemptCount;
        UpdatedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }

    private void Apply(NotificationFailedEvent e)
    {
        Status = DeliveryStatus.Failed;
        AttemptCount = e.AttemptCount;
        UpdatedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }
}
