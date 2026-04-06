using Whyce.Runtime.Middleware;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Middleware.PostPolicy;

/// <summary>
/// Post-policy idempotency guard. Prevents duplicate command processing.
/// Runs AFTER policy to avoid caching denied commands.
/// Key: CommandType + AggregateId (deterministic — same input produces same AggregateId).
/// CorrelationId is excluded because it changes per HTTP request; including it
/// would allow business-level duplicates with different correlation IDs.
/// </summary>
public sealed class IdempotencyMiddleware : IMiddleware
{
    private readonly IIdempotencyStore _idempotencyStore;

    public IdempotencyMiddleware(IIdempotencyStore idempotencyStore)
    {
        _idempotencyStore = idempotencyStore;
    }

    public async Task<CommandResult> ExecuteAsync(CommandContext context, object command, Func<Task<CommandResult>> next)
    {
        var idempotencyKey = $"{command.GetType().Name}:{context.AggregateId}";

        if (await _idempotencyStore.ExistsAsync(idempotencyKey))
        {
            return CommandResult.Failure("Duplicate command detected.");
        }

        var result = await next();

        if (result.IsSuccess)
        {
            await _idempotencyStore.MarkAsync(idempotencyKey);
        }

        return result;
    }
}
