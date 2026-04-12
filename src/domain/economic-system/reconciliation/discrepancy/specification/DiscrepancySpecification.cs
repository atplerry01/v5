using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;

public sealed class CanInvestigateSpecification : Specification<DiscrepancyAggregate>
{
    public override bool IsSatisfiedBy(DiscrepancyAggregate discrepancy)
    {
        return discrepancy.Status == DiscrepancyStatus.Open;
    }
}
