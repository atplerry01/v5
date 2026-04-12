using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed class HasJournalsSpecification : Specification<LedgerAggregate>
{
    public override bool IsSatisfiedBy(LedgerAggregate entity)
    {
        return entity.Journals.Count > 0;
    }
}
