using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Systems.Downstream.Operational.Sandbox.Todo;

public sealed class TodoIntentHandler : ITodoIntentHandler
{
    private static readonly DomainRoute TodoRoute = new("operational", "sandbox", "todo");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;

    public TodoIntentHandler(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
    }

    public async Task<TodoSystemResult> HandleCreateAsync(string title, string userId)
    {
        var todoId = _idGenerator.Generate($"todo:{userId}:{title}");
        var cmd = new CreateTodoCommand(todoId, title);
        var result = await _dispatcher.DispatchAsync(cmd, TodoRoute);
        return result.IsSuccess
            ? TodoSystemResult.Ok(todoId)
            : TodoSystemResult.Fail(result.Error ?? "Unknown error");
    }
}
