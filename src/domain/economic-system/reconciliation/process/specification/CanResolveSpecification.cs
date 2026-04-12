using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Process;

public sealed class CanResolveSpecification : Specification<ProcessAggregate>
{
    public override bool IsSatisfiedBy(ProcessAggregate process)
    {
        return process.Status == ReconciliationStatus.Matched
            || process.Status == ReconciliationStatus.Mismatched;
    }
}
