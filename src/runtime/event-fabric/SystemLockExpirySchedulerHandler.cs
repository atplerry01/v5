using LockCommands = Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Phase 8 B5 — scheduler-side dispatch logic for naturally-expired system
/// locks. Shape parity with <see cref="SanctionExpirySchedulerHandler"/>.
///
/// <para>
/// <b>Deterministic idempotency key.</b>
///   <c>system-lock-expiry:{LockId:N}:{ExpiresAt.UtcTicks}</c>
/// </para>
///
/// <para>
/// <b>Suspended locks.</b> These are filtered out at the query boundary
/// (status != 'Locked') rather than here — which matches the aggregate's
/// own <c>LockAggregate.Expire</c> invariant that rejects non-Locked
/// transitions. A suspended lock that is later Resumed re-enters the
/// candidate stream on the next scheduler tick (its projected Status
/// returns to 'Locked' and ExpiresAt is preserved from the original
/// Lock-time stamp).
/// </para>
///
/// <para>
/// <b>Failure semantics.</b> Same contract as the sanction handler:
/// exception → release the claim for retry; successful but failed
/// <see cref="CommandResult"/> (aggregate already Unlocked, Expired,
/// etc.) → leave claimed, let the next scan's status filter naturally
/// drop the row.
/// </para>
/// </summary>
public sealed class SystemLockExpirySchedulerHandler
{
    private const string IdempotencyKeyPrefix = "system-lock-expiry";

    private static readonly DomainRoute LockRoute =
        new("economic", "enforcement", "lock");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdempotencyStore _idempotencyStore;

    public SystemLockExpirySchedulerHandler(
        ISystemIntentDispatcher dispatcher,
        IIdempotencyStore idempotencyStore)
    {
        _dispatcher = dispatcher;
        _idempotencyStore = idempotencyStore;
    }

    public async Task HandleAsync(
        ExpirableLockCandidate candidate,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = BuildKey(candidate);
        var claimed = await _idempotencyStore.TryClaimAsync(idempotencyKey, cancellationToken);
        if (!claimed) return;

        try
        {
            var command = new LockCommands.ExpireSystemLockCommand(
                LockId: candidate.LockId,
                ExpiredAt: candidate.ExpiresAt);

            await _dispatcher.DispatchSystemAsync(command, LockRoute, cancellationToken);
        }
        catch
        {
            await _idempotencyStore.ReleaseAsync(idempotencyKey, cancellationToken);
            throw;
        }
    }

    private static string BuildKey(ExpirableLockCandidate candidate) =>
        $"{IdempotencyKeyPrefix}:{candidate.LockId:N}:{candidate.ExpiresAt.UtcTicks}";
}
