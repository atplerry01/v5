namespace Whyce.Shared.Contracts.Runtime;

public interface IWorkflowDispatcher
{
    Task<WorkflowResult> StartWorkflowAsync(string workflowName, object payload);
}
