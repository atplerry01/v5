namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed class BalancedJournalSpec
{
    public bool IsSatisfiedBy(decimal totalDebits, decimal totalCredits) => totalDebits == totalCredits;
}
