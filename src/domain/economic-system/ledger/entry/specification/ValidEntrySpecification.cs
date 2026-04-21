using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Entry;

public sealed class ValidEntrySpecification : Specification<LedgerEntryAggregate>
{
    public override bool IsSatisfiedBy(LedgerEntryAggregate entity)
    {
        if (entity.Amount.Value <= 0)
            return false;

        if (entity.JournalId.Value == Guid.Empty)
            return false;

        if (entity.AccountId.Value == Guid.Empty)
            return false;

        return true;
    }
}
