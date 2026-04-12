namespace Whycespace.Domain.BusinessSystem.Integration.Subscription;

public sealed record SubscriptionCreatedEvent(SubscriptionId SubscriptionId, SubscriptionTargetId TargetId);
