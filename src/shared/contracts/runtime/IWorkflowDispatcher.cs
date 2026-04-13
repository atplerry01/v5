namespace Whycespace.Shared.Contracts.Runtime;

public interface IWorkflowDispatcher
{
    Task<WorkflowResult> StartWorkflowAsync(string workflowName, object payload, DomainRoute route);
}
