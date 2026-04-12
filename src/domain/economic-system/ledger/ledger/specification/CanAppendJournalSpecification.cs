using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed class CanAppendJournalSpecification : Specification<LedgerAggregate>
{
    private readonly Guid _journalId;

    public CanAppendJournalSpecification(Guid journalId)
    {
        _journalId = journalId;
    }

    public override bool IsSatisfiedBy(LedgerAggregate entity)
    {
        if (_journalId == Guid.Empty)
            return false;

        return !entity.Journals.Any(j => j.JournalId == _journalId);
    }
}
