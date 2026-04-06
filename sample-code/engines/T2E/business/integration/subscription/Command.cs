namespace Whycespace.Engines.T2E.Business.Integration.Subscription;

public record SubscriptionCommand(
    string Action,
    string EntityId,
    object Payload
);
