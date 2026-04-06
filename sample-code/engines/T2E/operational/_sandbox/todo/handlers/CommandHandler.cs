using Whycespace.Shared.Contracts.Domain.Operational.Todo;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational._Sandbox.Todo;

public sealed class TodoCommandHandler
{
    private readonly TodoCreateEngine _createEngine;

    public TodoCommandHandler(ITodoDomainService todoDomainService)
    {
        _createEngine = new TodoCreateEngine(todoDomainService);
    }

    public Task<EngineResult> HandleAsync(
        TodoCommand command,
        EngineContext context,
        CancellationToken ct) => command switch
    {
        CreateTodoCommand create => _createEngine.ExecuteAsync(create, context, ct),
        _ => throw new System.NotSupportedException($"Unknown command: {command.GetType().Name}")
    };
}
