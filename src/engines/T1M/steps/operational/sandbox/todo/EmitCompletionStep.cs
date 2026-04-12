using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Engines.T1M.Steps.Operational.Sandbox.Todo;

public sealed class EmitCompletionStep : IWorkflowStep
{
    public string Name => "EmitCompletion";
    public WorkflowStepType StepType => WorkflowStepType.Completion;

    public Task<WorkflowStepResult> ExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
    {
        var todoId = context.StepOutputs.TryGetValue("CreateTodo", out var id) ? id : null;
        context.WorkflowOutput = todoId;

        return Task.FromResult(
            WorkflowStepResult.Success(todoId));
    }
}
