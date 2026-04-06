namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed class ObligationSettlementSpec
{
    public bool IsSatisfiedBy(ObligationAggregate obligation) =>
        obligation.Status == ObligationStatus.Active;
}
