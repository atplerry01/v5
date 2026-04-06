namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

public sealed class WorkflowAggregate : AggregateRoot
{
    public Guid DefinitionId { get; private set; }
    public string WorkflowType { get; private set; } = string.Empty;

    public static WorkflowAggregate Start(Guid id, Guid definitionId, string workflowType)
    {
        var aggregate = new WorkflowAggregate();
        aggregate.Id = id;
        aggregate.DefinitionId = definitionId;
        aggregate.WorkflowType = workflowType;
        aggregate.RaiseDomainEvent(new WorkflowStartedEvent(id, definitionId, workflowType));
        return aggregate;
    }
}
