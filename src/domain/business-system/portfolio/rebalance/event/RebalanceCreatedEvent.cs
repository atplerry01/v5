namespace Whycespace.Domain.BusinessSystem.Portfolio.Rebalance;

public sealed record RebalanceCreatedEvent(
    RebalanceId RebalanceId,
    RebalanceName RebalanceName);
