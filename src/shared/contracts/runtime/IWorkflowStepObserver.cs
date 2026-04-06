namespace Whyce.Shared.Contracts.Runtime;

public interface IWorkflowStepObserver
{
    Task OnStepCompletedAsync(WorkflowExecutionContext context, int stepIndex, string stepName);
    Task OnWorkflowCompletedAsync(WorkflowExecutionContext context);
    Task OnWorkflowFailedAsync(WorkflowExecutionContext context, string failedStep, string error);
}
