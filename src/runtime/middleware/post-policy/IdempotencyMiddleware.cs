using Whyce.Runtime.Middleware;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Middleware.PostPolicy;

/// <summary>
/// Post-policy idempotency guard. Prevents duplicate command processing.
/// Runs AFTER policy to avoid caching denied commands.
/// Key: CommandId (deterministic — derived from aggregateId + commandType +
/// full command signature in SystemIntentDispatcher). This ensures that two
/// structurally identical commands produce the same key while distinct commands
/// targeting the same aggregate (e.g. multiple list creates on one board)
/// produce distinct keys.
/// </summary>
public sealed class IdempotencyMiddleware : IMiddleware
{
    private readonly IIdempotencyStore _idempotencyStore;

    public IdempotencyMiddleware(IIdempotencyStore idempotencyStore)
    {
        _idempotencyStore = idempotencyStore;
    }

    public async Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = $"{command.GetType().Name}:{context.CommandId}";

        // phase1.5-S5.2.2 / KC-2 (IDEMPOTENCY-COALESCE-01): single
        // round-trip claim replaces the pre-KC-2 ExistsAsync +
        // MarkAsync two-step shape. Happy path = 1 event-store pool
        // acquisition (down from 2). Duplicate path = 1 acquisition.
        // Failure path = 2 acquisitions (claim + release rollback) —
        // matching the pre-KC-2 worst case but only on the failure
        // branch. The "mark only on success" semantics are preserved
        // exactly: a failed inner pipeline releases the claim so the
        // next attempt for the same key is not permanently blocked.
        //
        // phase1.5-S5.2.3 / TC-1: cancellationToken is forwarded to
        // next(ct). phase1.5-S5.2.3 / TC-5: the IIdempotencyStore
        // contract now also takes the token, so the underlying
        // PostgresIdempotencyStoreAdapter Execute*Async calls honor
        // cancellation end-to-end.
        if (!await _idempotencyStore.TryClaimAsync(idempotencyKey, cancellationToken))
        {
            return CommandResult.Failure("Duplicate command detected.");
        }

        CommandResult result;
        try
        {
            result = await next(cancellationToken);
        }
        catch
        {
            // Inner pipeline threw — release the claim before
            // re-throwing so a retry of the same logical command is
            // not blocked by an in-flight-failed claim row.
            // TC-5: rollback path also forwards CT, though it is only reached on the failure branch.
            await _idempotencyStore.ReleaseAsync(idempotencyKey, CancellationToken.None);
            throw;
        }

        if (!result.IsSuccess)
        {
            // TC-5: rollback path also forwards CT, though it is only reached on the failure branch.
            await _idempotencyStore.ReleaseAsync(idempotencyKey, CancellationToken.None);
        }

        return result;
    }
}
