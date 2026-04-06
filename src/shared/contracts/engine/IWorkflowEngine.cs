using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Shared.Contracts.Engine;

public interface IWorkflowEngine
{
    Task<WorkflowExecutionResult> ExecuteAsync(
        WorkflowDefinition definition,
        WorkflowExecutionContext context);
}
