namespace Whycespace.Engines.T2E.Business.Subscription.Usage;

public record UsageCommand(
    string Action,
    string EntityId,
    object Payload
);
