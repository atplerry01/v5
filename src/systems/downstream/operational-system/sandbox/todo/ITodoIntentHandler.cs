using Whyce.Shared.Contracts.Application.Todo;

namespace Whyce.Systems.Downstream.OperationalSystem.Sandbox.Todo;

public interface ITodoIntentHandler
{
    Task<TodoSystemResult> HandleAsync(CreateTodoIntent intent);
}
