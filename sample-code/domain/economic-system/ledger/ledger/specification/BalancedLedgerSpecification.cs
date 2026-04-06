namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed class BalancedLedgerSpecification
{
    public bool IsSatisfiedBy(IReadOnlyList<DebitCredit> entries)
    {
        var totalDebits = 0m;
        var totalCredits = 0m;

        for (var i = 0; i < entries.Count; i++)
        {
            totalDebits += entries[i].Debit;
            totalCredits += entries[i].Credit;
        }

        return totalDebits == totalCredits;
    }
}
