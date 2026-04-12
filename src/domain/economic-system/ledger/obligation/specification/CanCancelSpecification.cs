using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed class CanCancelSpecification : Specification<ObligationAggregate>
{
    public override bool IsSatisfiedBy(ObligationAggregate entity) =>
        entity.Status == ObligationStatus.Pending;
}
