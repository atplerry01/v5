namespace Whyce.Shared.Contracts.Projections.Orchestration.Workflow;

public interface IWorkflowExecutionProjectionStore
{
    Task<WorkflowExecutionReadModel?> GetAsync(Guid workflowExecutionId);
    Task UpsertAsync(WorkflowExecutionReadModel model);
}
