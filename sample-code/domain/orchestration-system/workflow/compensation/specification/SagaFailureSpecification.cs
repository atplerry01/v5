using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed class SagaFailureSpecification : Specification<SagaInstanceAggregate>
{
    public override bool IsSatisfiedBy(SagaInstanceAggregate saga)
    {
        return saga.State == SagaState.Failed
            || saga.State == SagaState.Compensating;
    }
}
