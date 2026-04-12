using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed class CanCompleteSpecification : Specification<SettlementAggregate>
{
    public override bool IsSatisfiedBy(SettlementAggregate entity) =>
        entity.Status == SettlementStatus.Initiated;
}
