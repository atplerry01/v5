using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

public sealed class JournalBalanceService
{
    public bool IsBalanced(JournalAggregate journal)
    {
        var totalDebit = GetTotalDebit(journal);
        var totalCredit = GetTotalCredit(journal);
        return totalDebit.Value == totalCredit.Value;
    }

    public Amount GetImbalance(JournalAggregate journal)
    {
        var totalDebit = GetTotalDebit(journal);
        var totalCredit = GetTotalCredit(journal);
        return new Amount(Math.Abs(totalDebit.Value - totalCredit.Value));
    }

    private static Amount GetTotalDebit(JournalAggregate journal)
    {
        var total = journal.Entries
            .Where(e => e.Direction == BookingDirection.Debit)
            .Sum(e => e.Amount.Value);
        return new Amount(total);
    }

    private static Amount GetTotalCredit(JournalAggregate journal)
    {
        var total = journal.Entries
            .Where(e => e.Direction == BookingDirection.Credit)
            .Sum(e => e.Amount.Value);
        return new Amount(total);
    }
}
