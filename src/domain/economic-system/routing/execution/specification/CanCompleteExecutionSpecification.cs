using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Routing.Execution;

public sealed class CanCompleteExecutionSpecification : Specification<ExecutionAggregate>
{
    public override bool IsSatisfiedBy(ExecutionAggregate execution)
    {
        return execution.Status == ExecutionStatus.Started;
    }
}
