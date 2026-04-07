namespace Whyce.Shared.Contracts.Projections.OrchestrationSystem.Workflow;

public interface IWorkflowExecutionProjectionStore
{
    Task<WorkflowExecutionReadModel?> GetAsync(Guid workflowExecutionId);
    Task UpsertAsync(WorkflowExecutionReadModel model);
}
