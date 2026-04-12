namespace Whycespace.Domain.BusinessSystem.Notification.Subscription;

public sealed record SubscriptionOptedInEvent(SubscriptionId SubscriptionId, SubscriptionTarget Target);
