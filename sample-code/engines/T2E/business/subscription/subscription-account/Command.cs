namespace Whycespace.Engines.T2E.Business.Subscription.SubscriptionAccount;

public record SubscriptionAccountCommand(
    string Action,
    string EntityId,
    object Payload
);
