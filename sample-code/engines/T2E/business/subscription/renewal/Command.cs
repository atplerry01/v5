namespace Whycespace.Engines.T2E.Business.Subscription.Renewal;

public record RenewalCommand(
    string Action,
    string EntityId,
    object Payload
);
