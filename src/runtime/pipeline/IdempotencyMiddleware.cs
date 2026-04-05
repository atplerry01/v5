using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Pipeline;

public sealed class IdempotencyMiddleware : IMiddleware
{
    private readonly IIdempotencyStore _idempotencyStore;

    public IdempotencyMiddleware(IIdempotencyStore idempotencyStore)
    {
        _idempotencyStore = idempotencyStore;
    }

    public async Task<CommandResult> ExecuteAsync(CommandContext context, object command, Func<Task<CommandResult>> next)
    {
        var idempotencyKey = $"{context.CorrelationId}:{command.GetType().Name}:{context.AggregateId}";

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

public interface IIdempotencyStore
{
    Task<bool> ExistsAsync(string key);
    Task MarkAsync(string key);
}
