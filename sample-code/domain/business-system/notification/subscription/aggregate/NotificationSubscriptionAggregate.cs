using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Primitive.Identity;

namespace Whycespace.Domain.BusinessSystem.Notification.Subscription;

public sealed class NotificationSubscriptionAggregate : AggregateRoot
{
    public SubscriptionId SubscriptionId { get; private set; }
    public IdentityId IdentityId { get; private set; } = default!;
    public string EventType { get; private set; } = string.Empty;
    public bool IsSubscribed { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static NotificationSubscriptionAggregate Create(Guid subscriptionId, IdentityId identityId, string eventType)
    {
        var subscription = new NotificationSubscriptionAggregate();
        subscription.Apply(new SubscriptionCreatedEvent(subscriptionId, identityId.Value, eventType));
        return subscription;
    }

    public void Unsubscribe()
    {
        if (!IsSubscribed)
            throw new DomainException(SubscriptionErrors.AlreadyUnsubscribed, "Already unsubscribed.");

        Apply(new SubscriptionCancelledEvent(Id));
    }

    public void Resubscribe()
    {
        if (IsSubscribed)
            throw new DomainException(SubscriptionErrors.AlreadySubscribed, "Already subscribed.");

        Apply(new SubscriptionReactivatedEvent(Id));
    }

    private void Apply(SubscriptionCreatedEvent e)
    {
        Id = e.SubscriptionId;
        SubscriptionId = new SubscriptionId(e.SubscriptionId);
        IdentityId = new IdentityId(e.IdentityId);
        EventType = e.EventType;
        IsSubscribed = true;
        CreatedAt = e.OccurredAt;
        UpdatedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }

    private void Apply(SubscriptionCancelledEvent e)
    {
        IsSubscribed = false;
        UpdatedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }

    private void Apply(SubscriptionReactivatedEvent e)
    {
        IsSubscribed = true;
        UpdatedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }
}
