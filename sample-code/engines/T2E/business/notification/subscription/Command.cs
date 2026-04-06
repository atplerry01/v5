namespace Whycespace.Engines.T2E.Business.Notification.Subscription;

public record SubscriptionCommand(
    string Action,
    string EntityId,
    object Payload
);
