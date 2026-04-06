namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public sealed class TreasuryComplianceSpec
{
    public bool IsSatisfiedBy(TreasuryAggregate treasury)
    {
        return treasury.Id != Guid.Empty;
    }
}
