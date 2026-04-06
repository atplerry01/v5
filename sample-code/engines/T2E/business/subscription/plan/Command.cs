namespace Whycespace.Engines.T2E.Business.Subscription.Plan;

public record PlanCommand(
    string Action,
    string EntityId,
    object Payload
);
