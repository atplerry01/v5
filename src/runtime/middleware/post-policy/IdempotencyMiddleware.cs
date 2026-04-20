using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Middleware.PostPolicy;

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

        // R1 §8 — surface the key on the context so audit, metrics, and
        // downstream middleware see evidence of idempotency tracking.
        if (context.IdempotencyKey is null)
        {
            context.IdempotencyKey = idempotencyKey;
        }

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
            // R1 §8 — duplicate signal surfaced via canonical shape. The
            // `IIdempotencyStore` contract does not return the previous
            // result today, so `AlreadyProcessed` cannot be reconstructed
            // verbatim; callers get IsDuplicate=true + the key + the
            // canonical ConcurrencyConflict category. Full previous-result
            // replay is a follow-up once the store contract exposes it.
            if (context.IdempotencyOutcome is null)
                context.IdempotencyOutcome = IdempotencyOutcome.Hit;
            return CommandResult.Failure(
                "Duplicate command detected.",
                RuntimeFailureCategory.ConcurrencyConflict)
                with
                {
                    IsDuplicate = true,
                    IdempotencyKey = idempotencyKey
                };
        }

        if (context.IdempotencyOutcome is null)
            context.IdempotencyOutcome = IdempotencyOutcome.Miss;

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
