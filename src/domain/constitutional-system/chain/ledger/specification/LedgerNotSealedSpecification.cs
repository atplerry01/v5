namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public static class LedgerNotSealedSpecification
{
    public static bool IsSatisfiedBy(LedgerStatus status) =>
        status != LedgerStatus.Sealed;
}
