using System.Diagnostics;
using Whycespace.Engines.T2E.OutboundEffects.Lifecycle;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.OutboundEffects;

/// <summary>
/// R3.B.4 / R-OUT-EFF-FINALITY-COMMAND-01 — sanctioned seam for async finality
/// transitions. Parallel to <see cref="OutboundEffectDispatcher"/> for the
/// schedule side: constructs lifecycle events via
/// <see cref="OutboundEffectLifecycleEventFactory"/> and emits them through
/// <see cref="IEventFabric.ProcessAsync"/>. No caller mutates the aggregate
/// directly; the aggregate refuses invalid source states on
/// <c>Apply</c>, so precondition violations surface as
/// <c>DomainInvariantViolationException</c>.
/// </summary>
public sealed class OutboundEffectFinalityService : IOutboundEffectFinalityService
{
    private readonly IOutboundEffectQueueStore _queueStore;
    private readonly IOutboundEffectAdapterRegistry? _adapterRegistry;
    private readonly OutboundEffectLifecycleEventFactory _lifecycleFactory;
    private readonly IEventFabric _eventFabric;
    private readonly IClock _clock;
    private readonly OutboundEffectsMeter _meter;
    private readonly OutboundEffectCompensationDispatcher? _compensationDispatcher;

    public OutboundEffectFinalityService(
        IOutboundEffectQueueStore queueStore,
        OutboundEffectLifecycleEventFactory lifecycleFactory,
        IEventFabric eventFabric,
        IClock clock,
        OutboundEffectsMeter meter,
        IOutboundEffectAdapterRegistry? adapterRegistry = null,
        OutboundEffectCompensationDispatcher? compensationDispatcher = null)
    {
        _queueStore = queueStore;
        _lifecycleFactory = lifecycleFactory;
        _eventFabric = eventFabric;
        _clock = clock;
        _meter = meter;
        _adapterRegistry = adapterRegistry;
        _compensationDispatcher = compensationDispatcher;
    }

    public async Task FinalizeAsync(
        Guid effectId,
        OutboundFinalityOutcome outcome,
        string evidenceDigest,
        string finalitySource,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(evidenceDigest))
            throw new ArgumentException("evidenceDigest is required.", nameof(evidenceDigest));
        if (string.IsNullOrWhiteSpace(finalitySource))
            throw new ArgumentException("finalitySource is required.", nameof(finalitySource));

        // R5.A Phase 3 / R-TRACE-OUTBOUND-FINALIZE-SPAN-01 — per-transition
        // span. Started AFTER arg validation so malformed-arg failures stay
        // on the caller's span. Exceptions re-propagate untouched; the
        // aggregate refuses invalid source states on Apply, so precondition
        // violations surface as DomainInvariantViolationException on the
        // parent span.
        using var activity = WhyceActivitySources.OutboundEffects.StartActivity(
            WhyceActivitySources.Spans.OutboundEffectFinalize,
            ActivityKind.Internal);
        activity?.SetTag(WhyceActivitySources.Attributes.TargetId, effectId);
        activity?.SetTag(WhyceActivitySources.Attributes.Outcome, outcome.ToString());
        activity?.SetTag(WhyceActivitySources.Attributes.FinalitySource, finalitySource);

        var entry = await _queueStore.GetAsync(effectId, cancellationToken)
            ?? throw new InvalidOperationException($"OutboundEffect {effectId} not found.");

        // R3.B.5 — finalize precondition: refuse if already in a terminal
        // state. Parallel to ReconcileAsync's status check. Guarantees
        // compensation emission is idempotent per trigger by forbidding
        // re-finalize of an already-finalized / cancelled / compensation-
        // requested row.
        if (entry.Status is OutboundEffectQueueStatus.Finalized
            or OutboundEffectQueueStatus.Cancelled
            or OutboundEffectQueueStatus.Reconciled
            or OutboundEffectQueueStatus.CompensationRequested
            or OutboundEffectQueueStatus.RetryExhausted)
        {
            throw new InvalidOperationException(
                $"Cannot finalize effect {effectId}: already in terminal status {entry.Status}.");
        }

        var now = _clock.UtcNow;
        var finalizedEvent = _lifecycleFactory.Finalized(
            effectId, outcome, evidenceDigest, now, finalitySource);

        // R3.B.5 / R-OUT-EFF-COMPENSATION-ATOMIC-01 — when the outcome +
        // shape require compensation, the compensation event is emitted
        // ATOMICALLY in the same fabric batch as the Finalized event. This
        // guarantees idempotency per trigger (a single call produces one of
        // each) and replay safety (the aggregate's Apply reconstructs both
        // transitions deterministically on replay).
        var shape = ResolveShape(entry.ProviderId);
        var requiresCompensation = OutboundEffectCompensationPolicy.RequiresCompensation(outcome, shape);

        var compensationEvent = requiresCompensation
            ? _lifecycleFactory.CompensationRequested(effectId, outcome)
            : null;

        var eventBatch = compensationEvent is not null
            ? new object[] { finalizedEvent, compensationEvent }
            : new object[] { finalizedEvent };

        var routeContext = RouteContext(effectId);
        await _eventFabric.ProcessAsync(eventBatch, routeContext, cancellationToken);

        var persistedStatus = compensationEvent is not null
            ? OutboundEffectQueueStatus.CompensationRequested
            : OutboundEffectQueueStatus.Finalized;

        await _queueStore.UpdateStatusAsync(
            effectId,
            persistedStatus,
            entry.AttemptCount,
            nextAttemptAt: now,
            ackDeadline: null,
            finalityDeadline: null,
            lastError: null,
            updatedAt: now,
            cancellationToken);

        _meter.FinalityDurationMs.Record(
            (now - entry.CreatedAt).TotalMilliseconds,
            new KeyValuePair<string, object?>("provider", entry.ProviderId),
            new KeyValuePair<string, object?>("effect_type", entry.EffectType),
            new KeyValuePair<string, object?>("outcome", outcome.ToString()));

        if (compensationEvent is not null && _compensationDispatcher is not null)
        {
            var signal = new OutboundEffectCompensationSignal(
                effectId, entry.EffectType, entry.ProviderId, outcome.ToString());
            await _compensationDispatcher.DispatchAsync(signal, cancellationToken);
        }

        activity?.SetTag(WhyceActivitySources.Attributes.ProviderId, entry.ProviderId);
        activity?.SetTag(WhyceActivitySources.Attributes.EffectType, entry.EffectType);
        activity?.SetTag(WhyceActivitySources.Attributes.CompensationEmitted, compensationEvent is not null);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    public async Task MarkReconciliationRequiredAsync(
        Guid effectId,
        OutboundReconciliationCause cause,
        string diagnosticEvidence,
        CancellationToken cancellationToken = default)
    {
        var entry = await _queueStore.GetAsync(effectId, cancellationToken)
            ?? throw new InvalidOperationException($"OutboundEffect {effectId} not found.");

        var now = _clock.UtcNow;
        var reconcilationEvent = _lifecycleFactory.ReconciliationRequired(effectId, cause, now);

        var routeContext = RouteContext(effectId);
        await _eventFabric.ProcessAsync(
            new object[] { reconcilationEvent }, routeContext, cancellationToken);

        await _queueStore.UpdateStatusAsync(
            effectId,
            OutboundEffectQueueStatus.ReconciliationRequired,
            entry.AttemptCount,
            nextAttemptAt: now,
            ackDeadline: null,
            finalityDeadline: null,
            lastError: diagnosticEvidence,
            updatedAt: now,
            cancellationToken);

        _meter.ReconciliationRequired.Add(1,
            new KeyValuePair<string, object?>("provider", entry.ProviderId),
            new KeyValuePair<string, object?>("effect_type", entry.EffectType),
            new KeyValuePair<string, object?>("cause", cause.ToString()));
    }

    public async Task ReconcileAsync(
        Guid effectId,
        OutboundFinalityOutcome outcome,
        string evidenceDigest,
        string reconcilerActorId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(evidenceDigest))
            throw new ArgumentException("evidenceDigest is required.", nameof(evidenceDigest));
        if (string.IsNullOrWhiteSpace(reconcilerActorId))
            throw new ArgumentException("reconcilerActorId is required.", nameof(reconcilerActorId));

        // R5.A Phase 3 / R-TRACE-OUTBOUND-RECONCILE-SPAN-01 — per
        // operator-driven reconcile span. The admin controller surface
        // invokes this; the span pairs with the admin
        // `runtime.admin.operator_action` span as a child of the HTTP
        // request trace. Sweeper-driven MarkReconciliationRequiredAsync
        // is intentionally NOT span-wrapped (background noise).
        using var activity = WhyceActivitySources.OutboundEffects.StartActivity(
            WhyceActivitySources.Spans.OutboundEffectReconcile,
            ActivityKind.Internal);
        activity?.SetTag(WhyceActivitySources.Attributes.TargetId, effectId);
        activity?.SetTag(WhyceActivitySources.Attributes.Outcome, outcome.ToString());
        activity?.SetTag(WhyceActivitySources.Attributes.ReconcilerActorId, reconcilerActorId);

        var entry = await _queueStore.GetAsync(effectId, cancellationToken)
            ?? throw new InvalidOperationException($"OutboundEffect {effectId} not found.");

        // R-OUT-EFF-RECONCILE-PRECONDITION-01 — reconcile is ONLY valid when
        // the row is currently reconciliation-required. Other statuses refuse
        // the operator command.
        if (entry.Status != OutboundEffectQueueStatus.ReconciliationRequired)
        {
            throw new InvalidOperationException(
                $"Cannot reconcile effect {effectId}: expected status " +
                $"ReconciliationRequired, got {entry.Status}.");
        }

        var now = _clock.UtcNow;
        var reconciledEvent = _lifecycleFactory.Reconciled(
            effectId, outcome, evidenceDigest, reconcilerActorId);

        // R3.B.5 — Reconciled(BusinessFailed | PartiallyCompleted-unsafe-shape)
        // emits compensation atomically with the Reconciled event.
        var shape = ResolveShape(entry.ProviderId);
        var requiresCompensation = OutboundEffectCompensationPolicy.RequiresCompensation(outcome, shape);

        var compensationEvent = requiresCompensation
            ? _lifecycleFactory.CompensationRequested(effectId, outcome)
            : null;

        var eventBatch = compensationEvent is not null
            ? new object[] { reconciledEvent, compensationEvent }
            : new object[] { reconciledEvent };

        var routeContext = RouteContext(effectId, actorId: reconcilerActorId);
        await _eventFabric.ProcessAsync(eventBatch, routeContext, cancellationToken);

        var persistedStatus = compensationEvent is not null
            ? OutboundEffectQueueStatus.CompensationRequested
            : OutboundEffectQueueStatus.Reconciled;

        await _queueStore.UpdateStatusAsync(
            effectId,
            persistedStatus,
            entry.AttemptCount,
            nextAttemptAt: now,
            ackDeadline: null,
            finalityDeadline: null,
            lastError: null,
            updatedAt: now,
            cancellationToken);

        if (compensationEvent is not null && _compensationDispatcher is not null)
        {
            var signal = new OutboundEffectCompensationSignal(
                effectId, entry.EffectType, entry.ProviderId, outcome.ToString());
            await _compensationDispatcher.DispatchAsync(signal, cancellationToken);
        }

        activity?.SetTag(WhyceActivitySources.Attributes.ProviderId, entry.ProviderId);
        activity?.SetTag(WhyceActivitySources.Attributes.EffectType, entry.EffectType);
        activity?.SetTag(WhyceActivitySources.Attributes.CompensationEmitted, compensationEvent is not null);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    /// <summary>
    /// R3.B.5 — resolve the adapter's <see cref="OutboundIdempotencyShape"/>.
    /// When the adapter is not registered (tests without a registry), default
    /// to <see cref="OutboundIdempotencyShape.ProviderIdempotent"/> which is
    /// the safest shape for compensation policy (emits on BusinessFailed
    /// only — the policy's irreducible minimum).
    /// </summary>
    private OutboundIdempotencyShape ResolveShape(string providerId)
    {
        if (_adapterRegistry is not null
            && _adapterRegistry.TryGet(providerId, out var adapter)
            && adapter is not null)
        {
            return adapter.IdempotencyShape;
        }
        return OutboundIdempotencyShape.ProviderIdempotent;
    }

    private static CommandContext RouteContext(Guid effectId, string actorId = "system/outbound-effect-finality")
        => new()
        {
            CorrelationId = effectId,
            CausationId = effectId,
            CommandId = effectId,
            TenantId = "system",
            ActorId = actorId,
            AggregateId = effectId,
            PolicyId = "system/outbound-effect-finality",
            Classification = "integration-system",
            Context = "outbound-effect",
            Domain = "outbound-effect",
        };
}
