using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Systems.Midstream.Wss.Workflows.Todo;

namespace Whyce.Systems.Downstream.OperationalSystem.Sandbox.Todo;

public sealed class TodoIntentHandler : ITodoIntentHandler
{
    private readonly IWorkflowDispatcher _workflowDispatcher;

    public TodoIntentHandler(IWorkflowDispatcher workflowDispatcher)
    {
        _workflowDispatcher = workflowDispatcher;
    }

    public async Task<TodoSystemResult> HandleAsync(CreateTodoIntent intent)
    {
        var result = await _workflowDispatcher.StartWorkflowAsync(
            TodoLifecycleWorkflow.CreateWorkflowName,
            intent);

        if (!result.IsSuccess)
        {
            return TodoSystemResult.Fail(result.Error ?? "Unknown error");
        }

        var todoId = result.Output is Guid id ? id : Guid.Empty;
        return TodoSystemResult.Ok(todoId);
    }
}
