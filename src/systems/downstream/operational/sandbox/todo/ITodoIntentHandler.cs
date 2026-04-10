using Whyce.Shared.Contracts.Operational.Sandbox.Todo;

namespace Whyce.Systems.Downstream.Operational.Sandbox.Todo;

public interface ITodoIntentHandler
{
    Task<TodoSystemResult> HandleAsync(CreateTodoIntent intent);
}
