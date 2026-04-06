using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Operational.Todo;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational._Sandbox.Todo;

public sealed class TodoCreateEngine : IEngine<CreateTodoCommand>
{
    private readonly ITodoDomainService _todoDomainService;

    public TodoCreateEngine(ITodoDomainService todoDomainService)
    {
        _todoDomainService = todoDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(
        CreateTodoCommand command,
        EngineContext context,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "operational.todo",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _todoDomainService.CreateAsync(
            execCtx,
            command.TodoId,
            command.Title,
            command.Description ?? string.Empty,
            command.Priority);

        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
