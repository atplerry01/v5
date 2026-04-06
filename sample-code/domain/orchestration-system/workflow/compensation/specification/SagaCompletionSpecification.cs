using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed class SagaCompletionSpecification : Specification<SagaInstanceAggregate>
{
    public override bool IsSatisfiedBy(SagaInstanceAggregate saga)
    {
        return saga.State == SagaState.Running
            && saga.CurrentStep is null
            && saga.CompletedSteps.Count > 0;
    }
}
