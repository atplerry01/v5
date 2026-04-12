using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Process;

public sealed class CanMatchSpecification : Specification<ProcessAggregate>
{
    public override bool IsSatisfiedBy(ProcessAggregate process)
    {
        return process.Status == ReconciliationStatus.Pending
            || process.Status == ReconciliationStatus.InProgress;
    }
}
