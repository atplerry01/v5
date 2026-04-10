using Whyce.Shared.Contracts.Operational.Sandbox.Todo;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Engines.T1M.Steps.Todo;

public sealed class ValidateIntentStep : IWorkflowStep
{
    public string Name => "ValidateIntent";
    public WorkflowStepType StepType => WorkflowStepType.Validation;

    public Task<WorkflowStepResult> ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (context.Payload is not CreateTodoIntent intent)
        {
            return Task.FromResult(
                WorkflowStepResult.Failure("Payload is not a valid CreateTodoIntent."));
        }

        if (string.IsNullOrWhiteSpace(intent.Title))
        {
            return Task.FromResult(
                WorkflowStepResult.Failure("Title is required."));
        }

        if (string.IsNullOrWhiteSpace(intent.UserId))
        {
            return Task.FromResult(
                WorkflowStepResult.Failure("UserId is required."));
        }

        return Task.FromResult(WorkflowStepResult.Success(intent));
    }
}
