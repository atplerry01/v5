namespace Whycespace.Engines.T2E.Business.Portfolio.Rebalance;

public record RebalanceCommand(
    string Action,
    string EntityId,
    object Payload
);
