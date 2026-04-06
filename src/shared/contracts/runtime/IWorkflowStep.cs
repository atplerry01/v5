using Whyce.Shared.Contracts.Engine;

namespace Whyce.Shared.Contracts.Runtime;

public interface IWorkflowStep
{
    string Name { get; }
    WorkflowStepType StepType { get; }
    Task<WorkflowStepResult> ExecuteAsync(WorkflowExecutionContext context);
}
