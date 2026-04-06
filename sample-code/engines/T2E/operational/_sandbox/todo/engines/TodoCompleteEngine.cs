using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Operational.Todo;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational._Sandbox.Todo;

/// <summary>
/// T2E engine for completing a Todo aggregate.
/// All business logic (invariant checks, terminal state guards) lives in the domain model.
/// </summary>
public sealed class TodoCompleteEngine : IEngine<CompleteTodoCommand>
{
    private readonly ITodoDomainService _todoDomainService;

    public TodoCompleteEngine(ITodoDomainService todoDomainService)
    {
        _todoDomainService = todoDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(
        CompleteTodoCommand command,
        EngineContext context,
        CancellationToken ct)
    {
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

        var result = await _todoDomainService.CompleteAsync(execCtx, command.TodoId);

        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
