using SanctionCommands = Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Phase 8 B5 — scheduler-side dispatch logic for naturally-expired
/// sanctions. The paired <c>SanctionExpirySchedulerWorker</c> owns the
/// periodic tick and clock read; this handler owns the per-candidate
/// idempotency-claim-and-dispatch contract, so the worker stays free of
/// command-shape knowledge and the handler stays trivially unit-testable.
///
/// <para>
/// <b>Deterministic idempotency key.</b>
///   <c>sanction-expiry:{SanctionId:N}:{ExpiresAt.UtcTicks}</c>
/// </para>
///
/// <para>
/// The key is a pure function of (aggregate, scheduled expiry). Two
/// different scheduler replicas scanning the same projection at the same
/// second, a restart mid-dispatch, or a replay of the scan loop all
/// produce the same key — the <see cref="IIdempotencyStore"/>'s atomic
/// claim is the single arbiter, and exactly one dispatch succeeds per
/// (SanctionId, ExpiresAt) pair. If a sanction's ExpiresAt is later
/// reissued for some as-yet-undefined future flow (not part of this
/// batch), the key would deterministically change — not an issue today.
/// </para>
///
/// <para>
/// <b>Failure semantics.</b> A dispatch that throws releases the claim
/// so the next scheduler tick can re-scan and retry cleanly. A dispatch
/// that returns a failed <see cref="CommandResult"/> (e.g. the aggregate
/// has already moved to Revoked between scan and dispatch) is LEFT
/// claimed — that (SanctionId, ExpiresAt) pair is terminally resolved
/// and should never be re-attempted. The next scan will not include the
/// row anyway (its projected Status is no longer Active).
/// </para>
/// </summary>
public sealed class SanctionExpirySchedulerHandler
{
    private const string IdempotencyKeyPrefix = "sanction-expiry";

    private static readonly DomainRoute SanctionRoute =
        new("economic", "enforcement", "sanction");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdempotencyStore _idempotencyStore;

    public SanctionExpirySchedulerHandler(
        ISystemIntentDispatcher dispatcher,
        IIdempotencyStore idempotencyStore)
    {
        _dispatcher = dispatcher;
        _idempotencyStore = idempotencyStore;
    }

    public async Task HandleAsync(
        ExpirableSanctionCandidate candidate,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = BuildKey(candidate);
        var claimed = await _idempotencyStore.TryClaimAsync(idempotencyKey, cancellationToken);
        if (!claimed) return;

        try
        {
            var command = new SanctionCommands.ExpireSanctionCommand(
                SanctionId: candidate.SanctionId,
                ExpiredAt: candidate.ExpiresAt);

            await _dispatcher.DispatchSystemAsync(command, SanctionRoute, cancellationToken);
        }
        catch
        {
            await _idempotencyStore.ReleaseAsync(idempotencyKey, cancellationToken);
            throw;
        }
    }

    private static string BuildKey(ExpirableSanctionCandidate candidate) =>
        $"{IdempotencyKeyPrefix}:{candidate.SanctionId:N}:{candidate.ExpiresAt.UtcTicks}";
}
