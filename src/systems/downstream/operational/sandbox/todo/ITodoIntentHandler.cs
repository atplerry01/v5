using Whyce.Shared.Contracts.Application.Todo;

namespace Whyce.Systems.Downstream.Operational.Sandbox.Todo;

public interface ITodoIntentHandler
{
    Task<TodoSystemResult> HandleAsync(CreateTodoIntent intent);
}
