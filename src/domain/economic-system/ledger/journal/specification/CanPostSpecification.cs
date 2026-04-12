using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

public sealed class CanPostSpecification : Specification<JournalAggregate>
{
    public override bool IsSatisfiedBy(JournalAggregate entity)
    {
        if (entity.Status != JournalStatus.Open)
            return false;

        if (entity.Entries.Count < 2)
            return false;

        var totalDebit = entity.Entries
            .Where(e => e.Direction == BookingDirection.Debit)
            .Sum(e => e.Amount.Value);

        var totalCredit = entity.Entries
            .Where(e => e.Direction == BookingDirection.Credit)
            .Sum(e => e.Amount.Value);

        return totalDebit == totalCredit;
    }
}
