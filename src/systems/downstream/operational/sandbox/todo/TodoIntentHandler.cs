using Whyce.Shared.Contracts.Operational.Sandbox.Todo;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Systems.Downstream.Operational.Sandbox.Todo;

public sealed class TodoIntentHandler : ITodoIntentHandler
{
    private static readonly DomainRoute TodoRoute = new("operational", "sandbox", "todo");

    private readonly IWorkflowDispatcher _workflowDispatcher;

    public TodoIntentHandler(IWorkflowDispatcher workflowDispatcher)
    {
        _workflowDispatcher = workflowDispatcher;
    }

    public async Task<TodoSystemResult> HandleAsync(CreateTodoIntent intent)
    {
        var result = await _workflowDispatcher.StartWorkflowAsync(
            TodoLifecycleWorkflowNames.Create,
            intent,
            TodoRoute);

        if (!result.IsSuccess)
        {
            return TodoSystemResult.Fail(result.Error ?? "Unknown error");
        }

        var todoId = result.Output is Guid id ? id : Guid.Empty;
        return TodoSystemResult.Ok(todoId);
    }
}
