using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Step;

public class WorkflowStepAggregate : AggregateRoot
{
    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new WorkflowStepCreatedEvent(id));
    }
}
