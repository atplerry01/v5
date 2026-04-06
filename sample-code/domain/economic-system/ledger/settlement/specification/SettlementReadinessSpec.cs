namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed class SettlementReadinessSpec
{
    public bool IsSatisfiedBy(SettlementAggregate settlement)
    {
        return settlement.Status != SettlementStatus.Completed
            && settlement.Amount is not null
            && !settlement.Amount.IsZero
            && !settlement.Amount.IsNegative;
    }
}
