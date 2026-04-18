using RestrictionCommands = Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using LockCommands = Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Phase 8 B4 — sanction-activation → enforcement saga reactor.
///
/// Observes <see cref="SanctionActivatedEventSchema"/> envelopes on the
/// sanction event topic and deterministically dispatches either
/// <c>ApplyRestrictionCommand</c> or <c>LockSystemCommand</c> — driven
/// by the <c>Enforcement.Kind</c> field stamped on the event (Phase 7
/// T7.10). The downstream aggregate id is the <c>EnforcementId</c>
/// derived deterministically at sanction-activation time, so the
/// reactor is a pure projection of what the event already defines —
/// no reinterpretation, no cross-aggregate hydration.
///
/// <para>
/// <b>Exactly-one routing.</b> The switch on <c>Enforcement.Kind</c>
/// picks ONE command type:
///   <list>
///     <item>Kind = Restriction → ApplyRestrictionCommand</item>
///     <item>Kind = Lock        → LockSystemCommand</item>
///   </list>
/// Never both, never neither. A sanction activation cannot produce
/// competing enforcement actions.
/// </para>
///
/// <para>
/// <b>Two-layer idempotency.</b>
///   (1) Envelope-level claim via <see cref="IIdempotencyStore"/> keyed on
///       <c>sanction-activation-enforcement:{EventId}</c> short-circuits
///       redelivered Kafka messages before the dispatcher is touched.
///   (2) The downstream <c>ApplyRestrictionHandler</c> /
///       <c>LockSystemHandler</c> (Phase 7 B4) load prior events via
///       <see cref="IEventStore"/> and short-circuit to no-op if the
///       aggregate already exists — same sanction activation → same
///       EnforcementId → same aggregate, never duplicated.
/// Together: at-least-once Kafka delivery produces exactly one
/// enforcement aggregate per sanction.
/// </para>
///
/// <para>
/// <b>V1 tolerance.</b> Events that predate T7.10 carry
/// <c>Enforcement = null</c> — no authoritative aggregate-id linkage
/// was stamped at activation time. The reactor SKIPS these events
/// explicitly (no default synthesis, no guessing) so replay of
/// historical sanction streams produces no ghost enforcement
/// aggregates. The aggregate's own Legacy cause synthesis covers the
/// replay invariant on the sanction side; enforcement linkage for
/// historical sanctions is out of scope for a write-side reactor.
/// </para>
///
/// <para>
/// <b>Scope / Reason / ExpiresAt.</b> The V2 event carries
/// <c>Enforcement.Kind</c> + <c>EnforcementId</c> + <c>SubjectId</c> +
/// <c>ActivatedAt</c> — enough for deterministic aggregate-id + cause +
/// timestamp. Scope defaults to <c>"System"</c> (the broadest scope a
/// sanction-driven enforcement should plausibly assume) and Reason is
/// synthesised from the SanctionId. ExpiresAt on Lock is left null —
/// the Activated event does not carry the Period.ExpiresAt from
/// Issuance, and a projection lookup would introduce non-determinism
/// against reactor-vs-projection consumer lag. Narrower scopes or
/// bounded ExpiresAt require extending the Phase 7
/// <see cref="SanctionActivatedEventSchema"/> to carry them on the
/// wire — out of B4 scope.
/// </para>
/// </summary>
public sealed class SanctionActivationEnforcementHandler
{
    private const string IdempotencyKeyPrefix = "sanction-activation-enforcement";
    private const string EnforcementKindRestriction = "Restriction";
    private const string EnforcementKindLock = "Lock";
    private const string SanctionCauseKind = "Sanction";
    private const string DefaultScope = "System";

    private static readonly DomainRoute RestrictionRoute =
        new("economic", "enforcement", "restriction");

    private static readonly DomainRoute LockRoute =
        new("economic", "enforcement", "lock");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdempotencyStore _idempotencyStore;

    public SanctionActivationEnforcementHandler(
        ISystemIntentDispatcher dispatcher,
        IIdempotencyStore idempotencyStore)
    {
        _dispatcher = dispatcher;
        _idempotencyStore = idempotencyStore;
    }

    public async Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        if (envelope.Payload is not SanctionActivatedEventSchema activated)
        {
            // Non-activation events on the sanction topic are observation-only
            // for this reactor (Issued/Expired/Revoked have their own
            // downstream consumers in follow-up batches).
            return;
        }

        if (activated.Enforcement is null)
        {
            // V1 event — no authoritative EnforcementRef. Skip explicitly;
            // replay of legacy streams must not produce ghost enforcement
            // aggregates with synthesised ids.
            return;
        }

        var idempotencyKey = $"{IdempotencyKeyPrefix}:{envelope.EventId}";
        var claimed = await _idempotencyStore.TryClaimAsync(idempotencyKey, cancellationToken);
        if (!claimed) return;

        try
        {
            await DispatchForKind(activated, cancellationToken);
        }
        catch
        {
            // Release the claim so a genuine retry can proceed. The worker
            // leaves the Kafka offset un-committed on exception, so the
            // message will be redelivered — the second attempt will claim
            // cleanly and the downstream handler's prior-event check will
            // short-circuit if the enforcement aggregate already exists.
            await _idempotencyStore.ReleaseAsync(idempotencyKey, cancellationToken);
            throw;
        }
    }

    private Task DispatchForKind(SanctionActivatedEventSchema activated, CancellationToken ct)
    {
        var enforcement = activated.Enforcement!;

        return enforcement.Kind switch
        {
            EnforcementKindRestriction => DispatchApplyRestriction(activated, ct),
            EnforcementKindLock        => DispatchLockSystem(activated, ct),
            _ => throw new InvalidOperationException(
                $"Unknown EnforcementRef.Kind '{enforcement.Kind}' on SanctionActivatedEvent for sanction {activated.AggregateId:N}. " +
                "Valid values are 'Restriction' or 'Lock'.")
        };
    }

    private Task DispatchApplyRestriction(SanctionActivatedEventSchema activated, CancellationToken ct)
    {
        var enforcement = activated.Enforcement!;
        var command = new RestrictionCommands.ApplyRestrictionCommand(
            RestrictionId: enforcement.EnforcementId,
            SubjectId: activated.SubjectId,
            Scope: DefaultScope,
            Reason: BuildReason(activated),
            AppliedAt: activated.ActivatedAt)
        {
            Cause = new RestrictionCommands.EnforcementCauseDto(
                Kind: SanctionCauseKind,
                CauseReferenceId: activated.AggregateId,
                Detail: BuildCauseDetail(activated)),
        };

        return _dispatcher.DispatchSystemAsync(command, RestrictionRoute, ct);
    }

    private Task DispatchLockSystem(SanctionActivatedEventSchema activated, CancellationToken ct)
    {
        var enforcement = activated.Enforcement!;
        var command = new LockCommands.LockSystemCommand(
            LockId: enforcement.EnforcementId,
            SubjectId: activated.SubjectId,
            Scope: DefaultScope,
            Reason: BuildReason(activated),
            LockedAt: activated.ActivatedAt)
        {
            Cause = new LockCommands.EnforcementCauseDto(
                Kind: SanctionCauseKind,
                CauseReferenceId: activated.AggregateId,
                Detail: BuildCauseDetail(activated)),
            // ExpiresAt deliberately left null — the Activated event does
            // not carry Period.ExpiresAt, and projection-backed hydration
            // would introduce reactor-vs-projection race. The Lock runs
            // until explicit Unlock unless an extended event schema
            // later carries ExpiresAt on the wire.
            ExpiresAt = null,
        };

        return _dispatcher.DispatchSystemAsync(command, LockRoute, ct);
    }

    private static string BuildReason(SanctionActivatedEventSchema activated) =>
        $"Sanction activation: {activated.AggregateId:N}";

    private static string BuildCauseDetail(SanctionActivatedEventSchema activated) =>
        $"Sanction {activated.AggregateId:N} activated against subject {activated.SubjectId:N} at {activated.ActivatedAt:O}";
}
