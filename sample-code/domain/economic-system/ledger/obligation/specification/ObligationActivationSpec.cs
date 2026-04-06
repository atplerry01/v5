namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed class ObligationActivationSpec
{
    public bool IsSatisfiedBy(ObligationAggregate obligation) =>
        obligation.Status == ObligationStatus.Created;
}
