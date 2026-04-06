namespace Whycespace.Engines.T2E.Decision.Governance.GovernanceCycle;

public record GovernanceCycleCommand(
    string Action,
    string EntityId,
    object Payload
);
