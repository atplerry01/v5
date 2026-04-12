using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

public sealed class IsBalancedSpecification : Specification<JournalAggregate>
{
    public override bool IsSatisfiedBy(JournalAggregate entity)
    {
        var totalDebit = entity.Entries
            .Where(e => e.Direction == BookingDirection.Debit)
            .Sum(e => e.Amount.Value);

        var totalCredit = entity.Entries
            .Where(e => e.Direction == BookingDirection.Credit)
            .Sum(e => e.Amount.Value);

        return totalDebit == totalCredit;
    }
}
