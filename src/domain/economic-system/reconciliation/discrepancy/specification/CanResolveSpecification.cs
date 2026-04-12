using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;

public sealed class CanResolveSpecification : Specification<DiscrepancyAggregate>
{
    public override bool IsSatisfiedBy(DiscrepancyAggregate discrepancy)
    {
        return discrepancy.Status == DiscrepancyStatus.Open
            || discrepancy.Status == DiscrepancyStatus.Investigating;
    }
}
