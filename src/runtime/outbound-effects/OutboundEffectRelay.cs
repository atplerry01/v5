using System.Diagnostics;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Engines.T2E.OutboundEffects.Lifecycle;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 / R-OUT-EFF-PROHIBITION-03 — the sole caller of
/// <see cref="IOutboundEffectAdapter.DispatchAsync"/>. Claims ready queue
/// rows, invokes the registered adapter within a per-attempt linked CTS
/// bounded by <c>DispatchTimeoutMs</c>, translates the six-outcome adapter
/// result into lifecycle events, and advances the queue row.
///
/// <para><b>R3.B.2 additions:</b> resolves per-provider
/// <see cref="ICircuitBreaker"/> via <see cref="ICircuitBreakerRegistry"/>
/// under the name <c>outbound.{providerId}</c> (parent design §6.2). When
/// present, the adapter call is wrapped in the breaker;
/// <see cref="CircuitBreakerOpenException"/> is classified Transient but
/// <b>does NOT consume the retry budget</b> — the provider was never called,
/// so the attempt counter is held.</para>
///
/// <para>The relay is a pure class. The hosted-service shell in
/// <c>src/platform/host/adapters/outbound-effects/</c> drives
/// <see cref="PollOnceAsync"/> in a loop.</para>
/// </summary>
public sealed class OutboundEffectRelay
{
    public const string BreakerNamePrefix = "outbound.";

    private readonly IOutboundEffectQueueStore _queueStore;
    private readonly IOutboundEffectAdapterRegistry _adapterRegistry;
    private readonly IOutboundEffectOptionsRegistry _optionsRegistry;
    private readonly OutboundEffectLifecycleEventFactory _lifecycleFactory;
    private readonly IEventFabric _eventFabric;
    private readonly IClock _clock;
    private readonly IRandomProvider _randomProvider;
    private readonly OutboundEffectsMeter _meter;
    private readonly OutboundEffectRelayOptions _options;
    private readonly ICircuitBreakerRegistry? _breakerRegistry;

    public OutboundEffectRelay(
        IOutboundEffectQueueStore queueStore,
        IOutboundEffectAdapterRegistry adapterRegistry,
        IOutboundEffectOptionsRegistry optionsRegistry,
        OutboundEffectLifecycleEventFactory lifecycleFactory,
        IEventFabric eventFabric,
        IClock clock,
        IRandomProvider randomProvider,
        OutboundEffectsMeter meter,
        OutboundEffectRelayOptions options,
        ICircuitBreakerRegistry? breakerRegistry = null,
        OutboundEffectCompensationDispatcher? compensationDispatcher = null)
    {
        _queueStore = queueStore;
        _adapterRegistry = adapterRegistry;
        _optionsRegistry = optionsRegistry;
        _lifecycleFactory = lifecycleFactory;
        _eventFabric = eventFabric;
        _clock = clock;
        _randomProvider = randomProvider;
        _meter = meter;
        _options = options;
        _breakerRegistry = breakerRegistry;
        _compensationDispatcher = compensationDispatcher;
    }

    private readonly OutboundEffectCompensationDispatcher? _compensationDispatcher;

    /// <summary>
    /// Claim up to <c>BatchSize</c> ready rows and dispatch each. Returns the
    /// number of rows processed.
    /// </summary>
    public async Task<int> PollOnceAsync(CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var claimed = await _queueStore.ClaimReadyAsync(
            _options.HostId, _options.BatchSize, now, cancellationToken);

        if (claimed.Count == 0) return 0;

        var processed = 0;
        foreach (var entry in claimed)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DispatchOneAsync(entry, cancellationToken);
            processed++;
        }
        return processed;
    }

    private async Task DispatchOneAsync(OutboundEffectQueueEntry entry, CancellationToken ct)
    {
        // R5.A Phase 3 / R-TRACE-OUTBOUND-DISPATCH-SPAN-01 — per-attempt
        // span (a retrying effect emits multiple dispatch spans, one per
        // attempt). Started at method entry so provider-missing /
        // options-missing branches also leave a span trail with the
        // canonical outcome tag. Span closes on every return path.
        using var activity = WhyceActivitySources.OutboundEffects.StartActivity(
            WhyceActivitySources.Spans.OutboundEffectDispatch,
            ActivityKind.Client);
        activity?.SetTag(WhyceActivitySources.Attributes.TargetId, entry.EffectId);
        activity?.SetTag(WhyceActivitySources.Attributes.ProviderId, entry.ProviderId);
        activity?.SetTag(WhyceActivitySources.Attributes.EffectType, entry.EffectType);
        activity?.SetTag(WhyceActivitySources.Attributes.AttemptNumber, entry.AttemptCount + 1);

        if (!_adapterRegistry.TryGet(entry.ProviderId, out var adapter) || adapter is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "provider_not_registered");
            activity?.SetTag(WhyceActivitySources.Attributes.Outcome, "provider_not_registered");
            var failureNow = _clock.UtcNow;
            await _queueStore.UpdateStatusAsync(
                entry.EffectId,
                OutboundEffectQueueStatus.TransientFailed,
                entry.AttemptCount,
                nextAttemptAt: failureNow.AddSeconds(30),
                ackDeadline: entry.AckDeadline,
                finalityDeadline: entry.FinalityDeadline,
                lastError: OutboundEffectErrors.ProviderNotRegistered,
                updatedAt: failureNow,
                ct);
            return;
        }

        if (!_optionsRegistry.TryGet(entry.ProviderId, out var options) || options is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "options_missing");
            activity?.SetTag(WhyceActivitySources.Attributes.Outcome, "options_missing");
            var failureNow = _clock.UtcNow;
            await _queueStore.UpdateStatusAsync(
                entry.EffectId,
                OutboundEffectQueueStatus.TransientFailed,
                entry.AttemptCount,
                nextAttemptAt: failureNow.AddSeconds(30),
                ackDeadline: entry.AckDeadline,
                finalityDeadline: entry.FinalityDeadline,
                lastError: OutboundEffectErrors.OptionsMissing,
                updatedAt: failureNow,
                ct);
            return;
        }

        var breaker = _breakerRegistry?.TryGet(BreakerNamePrefix + entry.ProviderId);

        var attemptNumber = entry.AttemptCount + 1;
        var dispatchStartedAt = _clock.UtcNow;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromMilliseconds(options.DispatchTimeoutMs));

        var dispatchContext = new OutboundEffectDispatchContext(
            entry.EffectId,
            new OutboundIdempotencyKey(entry.IdempotencyKey),
            attemptNumber,
            entry.Payload,
            CorrelationId: entry.EffectId,
            CausationId: entry.EffectId,
            ActorId: "system/outbound-effect-relay",
            DispatchTimeout: TimeSpan.FromMilliseconds(options.DispatchTimeoutMs));

        OutboundAdapterResult result;
        bool breakerShortCircuit = false;
        TimeSpan breakerRetryAfter = TimeSpan.Zero;

        try
        {
            if (breaker is null)
            {
                result = await adapter.DispatchAsync(dispatchContext, linkedCts.Token);
            }
            else
            {
                result = await breaker.ExecuteAsync(
                    innerCt => adapter.DispatchAsync(dispatchContext, innerCt),
                    linkedCts.Token);
            }
        }
        catch (CircuitBreakerOpenException breakerEx)
        {
            // Breaker short-circuit — parent design §6.2: the dispatch attempt
            // does NOT count against the retry budget because the provider was
            // never called. attempt_count stays at entry.AttemptCount below.
            breakerShortCircuit = true;
            breakerRetryAfter = TimeSpan.FromSeconds(Math.Max(1, breakerEx.RetryAfterSeconds));
            result = new OutboundAdapterResult.DispatchFailedPreAcceptance(
                OutboundAdapterClassification.Transient,
                $"breaker_open:{breakerEx.BreakerName}",
                RetryAfter: breakerRetryAfter);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            result = new OutboundAdapterResult.DispatchFailedPreAcceptance(
                OutboundAdapterClassification.Transient,
                "dispatch_timeout");
        }
        catch (Exception ex)
        {
            result = new OutboundAdapterResult.DispatchFailedPreAcceptance(
                OutboundAdapterClassification.Transient,
                $"adapter_exception:{ex.GetType().Name}:{ex.Message}");
        }

        var dispatchCompletedAt = _clock.UtcNow;

        var outcomeTag = OutcomeTag(result);
        _meter.DispatchDurationMs.Record(
            (dispatchCompletedAt - dispatchStartedAt).TotalMilliseconds,
            new KeyValuePair<string, object?>("provider", entry.ProviderId),
            new KeyValuePair<string, object?>("effect_type", entry.EffectType),
            new KeyValuePair<string, object?>("outcome", outcomeTag));

        // R5.A Phase 3 — stamp the outcome tag + span status. The OutcomeTag
        // helper classifies the six-variant adapter result into a
        // low-cardinality canonical string matching the meter's outcome
        // dimension, so span + metric stay aligned.
        activity?.SetTag(WhyceActivitySources.Attributes.Outcome, outcomeTag);

        activity?.SetStatus(
            result is OutboundAdapterResult.Acknowledged or
                     OutboundAdapterResult.DispatchedWithoutProviderOperationId or
                     OutboundAdapterResult.FinalizedSuccess
                ? ActivityStatusCode.Ok
                : ActivityStatusCode.Error,
            outcomeTag);

        await TranslateAndPersistAsync(
            entry, adapter, options, attemptNumber,
            dispatchStartedAt, dispatchCompletedAt, result,
            breakerShortCircuit, breakerRetryAfter, ct);
    }

    private async Task TranslateAndPersistAsync(
        OutboundEffectQueueEntry entry,
        IOutboundEffectAdapter adapter,
        OutboundEffectOptions options,
        int attemptNumber,
        DateTimeOffset dispatchStartedAt,
        DateTimeOffset dispatchCompletedAt,
        OutboundAdapterResult result,
        bool breakerShortCircuit,
        TimeSpan breakerRetryAfter,
        CancellationToken ct)
    {
        var events = new List<object>();
        string newStatus;
        DateTimeOffset? newAckDeadline = entry.AckDeadline;
        DateTimeOffset? newFinalityDeadline = entry.FinalityDeadline;
        DateTimeOffset nextAttemptAt = _clock.UtcNow;
        string? lastError = entry.LastError;
        var now = _clock.UtcNow;
        OutboundEffectCompensationSignal? compensationSignalToDispatch = null;

        switch (result)
        {
            case OutboundAdapterResult.Acknowledged acked:
                events.Add(_lifecycleFactory.Dispatched(
                    entry.EffectId, attemptNumber, dispatchStartedAt, dispatchCompletedAt));
                events.Add(_lifecycleFactory.Acknowledged(
                    entry.EffectId, acked.ProviderOperation, acked.AckPayloadDigest));
                newStatus = OutboundEffectQueueStatus.Acknowledged;
                newAckDeadline = null;
                newFinalityDeadline = now.AddMilliseconds(options.FinalityWindowMs);
                break;

            case OutboundAdapterResult.DispatchedWithoutProviderOperationId dispatched:
                events.Add(_lifecycleFactory.Dispatched(
                    entry.EffectId, attemptNumber, dispatchStartedAt, dispatchCompletedAt,
                    dispatched.TransportEvidence));
                newStatus = OutboundEffectQueueStatus.Dispatched;
                newAckDeadline = now.AddMilliseconds(options.AckTimeoutMs);
                break;

            case OutboundAdapterResult.FinalizedSuccess success:
                events.Add(_lifecycleFactory.Dispatched(
                    entry.EffectId, attemptNumber, dispatchStartedAt, dispatchCompletedAt));
                events.Add(_lifecycleFactory.Acknowledged(
                    entry.EffectId, success.ProviderOperation));
                events.Add(_lifecycleFactory.Finalized(
                    entry.EffectId,
                    OutboundFinalityOutcome.Succeeded,
                    success.FinalityEvidence,
                    now,
                    "SynchronousAck"));
                newStatus = OutboundEffectQueueStatus.Finalized;
                break;

            case OutboundAdapterResult.FinalizedFailure failure:
                events.Add(_lifecycleFactory.Dispatched(
                    entry.EffectId, attemptNumber, dispatchStartedAt, dispatchCompletedAt));
                if (failure.ProviderOperation is not null)
                {
                    events.Add(_lifecycleFactory.Acknowledged(
                        entry.EffectId, failure.ProviderOperation));
                }
                events.Add(_lifecycleFactory.Finalized(
                    entry.EffectId,
                    OutboundFinalityOutcome.BusinessFailed,
                    $"{failure.FailureCode}:{failure.FailureReason}",
                    now,
                    "SynchronousAck"));
                newStatus = OutboundEffectQueueStatus.Finalized;
                break;

            case OutboundAdapterResult.ReconciliationRequired reconciliation:
                events.Add(_lifecycleFactory.Dispatched(
                    entry.EffectId, attemptNumber, dispatchStartedAt, dispatchCompletedAt));
                events.Add(_lifecycleFactory.ReconciliationRequired(
                    entry.EffectId, reconciliation.Cause, now));
                newStatus = OutboundEffectQueueStatus.ReconciliationRequired;
                lastError = reconciliation.DiagnosticEvidence;
                _meter.ReconciliationRequired.Add(1,
                    new KeyValuePair<string, object?>("provider", entry.ProviderId),
                    new KeyValuePair<string, object?>("effect_type", entry.EffectType),
                    new KeyValuePair<string, object?>("cause", reconciliation.Cause.ToString()));
                break;

            case OutboundAdapterResult.DispatchFailedPreAcceptance failed:
                if (breakerShortCircuit)
                {
                    // R3.B.2 — breaker-open short-circuit. Provider was never
                    // called; attempt counter is NOT advanced. Reschedule at
                    // breaker's suggested retry window so next poll revisits
                    // when the breaker window is likely to have elapsed.
                    nextAttemptAt = now.Add(breakerRetryAfter);
                    newStatus = OutboundEffectQueueStatus.TransientFailed;
                    lastError = failed.Reason;
                    // No lifecycle event emitted — breaker-open did not attempt
                    // an outbound call; emitting DispatchFailedEvent would
                    // inflate attempt_count evidence beyond real attempts.
                    break;
                }

                events.Add(_lifecycleFactory.DispatchFailed(
                    entry.EffectId, attemptNumber, failed.Classification, failed.Reason,
                    failed.RetryAfter is { } ra ? (int)ra.TotalMilliseconds : null));
                lastError = failed.Reason;

                // R3.B.3 / R-OUT-EFF-AMBIGUOUS-ATMOSTONCE-01 — Ambiguous
                // dispatch on a shape that cannot safely retry post-dispatch
                // (AtMostOnceRequired, CompensatableOnly) routes to
                // ReconciliationRequired so an operator (or a future
                // automated reconciler) resolves the real provider state.
                // Routing through RetryExhausted here would terminally
                // refuse the work while the provider may already have
                // acted — a correctness defect the design forbids.
                if (failed.Classification == OutboundAdapterClassification.Ambiguous
                    && IsAmbiguousUnsafeToRetry(adapter.IdempotencyShape))
                {
                    events.Add(_lifecycleFactory.ReconciliationRequired(
                        entry.EffectId,
                        OutboundReconciliationCause.DispatchAmbiguous,
                        now));
                    newStatus = OutboundEffectQueueStatus.ReconciliationRequired;
                    _meter.ReconciliationRequired.Add(1,
                        new KeyValuePair<string, object?>("provider", entry.ProviderId),
                        new KeyValuePair<string, object?>("effect_type", entry.EffectType),
                        new KeyValuePair<string, object?>("cause", nameof(OutboundReconciliationCause.DispatchAmbiguous)));
                    break;
                }

                var eligible = IsRetryEligible(failed.Classification, adapter.IdempotencyShape, attemptNumber, options);
                if (eligible)
                {
                    var backoffMs = ComputeBackoffMs(options, attemptNumber, entry.EffectId, failed.RetryAfter, _randomProvider);
                    nextAttemptAt = now.AddMilliseconds(backoffMs);
                    events.Add(_lifecycleFactory.RetryAttempted(
                        entry.EffectId, attemptNumber, nextAttemptAt, backoffMs, failed.Classification));
                    newStatus = OutboundEffectQueueStatus.TransientFailed;
                    _meter.RetryAttempts.Add(1,
                        new KeyValuePair<string, object?>("provider", entry.ProviderId),
                        new KeyValuePair<string, object?>("effect_type", entry.EffectType));
                }
                else
                {
                    events.Add(_lifecycleFactory.RetryExhausted(
                        entry.EffectId, attemptNumber, failed.Classification));
                    _meter.RetryExhausted.Add(1,
                        new KeyValuePair<string, object?>("provider", entry.ProviderId),
                        new KeyValuePair<string, object?>("effect_type", entry.EffectType));

                    // R3.B.5 / R-OUT-EFF-COMPENSATION-EMIT-01 — retry
                    // exhaustion always emits compensation. The caller may
                    // have prepared local state in anticipation of the
                    // outbound effect; retry-exhausted means the effect did
                    // not complete, and the caller needs to reverse that
                    // state. Emitted atomically in the same fabric batch as
                    // the RetryExhausted event — one trigger, one
                    // compensation emission, replay-safe.
                    var exhaustionOutcome = OutboundFinalityOutcome.BusinessFailed;
                    events.Add(_lifecycleFactory.CompensationRequested(
                        entry.EffectId, exhaustionOutcome));
                    newStatus = OutboundEffectQueueStatus.CompensationRequested;
                    compensationSignalToDispatch = new OutboundEffectCompensationSignal(
                        entry.EffectId, entry.EffectType, entry.ProviderId,
                        TriggeringOutcome: nameof(OutboundEffectQueueStatus.RetryExhausted));
                }
                break;

            default:
                throw new InvalidOperationException(
                    $"Unhandled OutboundAdapterResult variant: {result.GetType().Name}");
        }

        // Relay-origin context: the relay emits lifecycle events directly
        // through the event fabric (no command dispatch). SYSTEM-ORIGIN-BYPASS-01
        // reserves IsSystem=true for ISystemIntentDispatcher only, so the
        // relay leaves IsSystem=false; enforcement guards do not apply to
        // pure event emissions.
        var routeContext = new CommandContext
        {
            CorrelationId = entry.EffectId,
            CausationId = entry.EffectId,
            CommandId = entry.EffectId,
            TenantId = "system",
            ActorId = "system/outbound-effect-relay",
            AggregateId = entry.EffectId,
            PolicyId = "system/outbound-effect-relay",
            Classification = "integration-system",
            Context = "outbound-effect",
            Domain = "outbound-effect",
        };
        if (events.Count > 0)
        {
            await _eventFabric.ProcessAsync(events, routeContext, ct);
        }

        // R3.B.2 — breaker-open short-circuit preserves attempt_count.
        var persistedAttemptCount = breakerShortCircuit ? entry.AttemptCount : attemptNumber;

        await _queueStore.UpdateStatusAsync(
            entry.EffectId,
            newStatus,
            persistedAttemptCount,
            nextAttemptAt,
            newAckDeadline,
            newFinalityDeadline,
            lastError,
            _clock.UtcNow,
            ct);

        // R3.B.5 — fire compensation handlers AFTER the lifecycle event + queue
        // update are durable. Handler failure must not roll back the emission.
        if (compensationSignalToDispatch is not null && _compensationDispatcher is not null)
        {
            await _compensationDispatcher.DispatchAsync(compensationSignalToDispatch, ct);
        }
    }

    private static bool IsRetryEligible(
        OutboundAdapterClassification classification,
        OutboundIdempotencyShape shape,
        int attemptNumber,
        OutboundEffectOptions options)
    {
        if (attemptNumber >= options.MaxAttempts) return false;
        return classification switch
        {
            OutboundAdapterClassification.Transient => true,
            OutboundAdapterClassification.Terminal => false,
            OutboundAdapterClassification.Ambiguous =>
                shape is OutboundIdempotencyShape.ProviderIdempotent or OutboundIdempotencyShape.NaturalKeyIdempotent,
            _ => false,
        };
    }

    /// <summary>
    /// R3.B.3 / R-OUT-EFF-BACKOFF-DET-01 — replay-deterministic backoff. The
    /// function is pure: given the same <paramref name="options"/>,
    /// <paramref name="attemptNumber"/>, <paramref name="effectId"/>,
    /// <paramref name="providerRetryAfter"/>, and
    /// <paramref name="randomProvider"/>, it returns the same millisecond
    /// value on every run. No wall-clock reads, no hidden entropy.
    ///
    /// <para>Formula:
    /// <list type="number">
    ///   <item>If <paramref name="providerRetryAfter"/> is present, honor it
    ///         capped at <see cref="OutboundEffectOptions.MaxBackoffMs"/>.</item>
    ///   <item>Otherwise compute
    ///         <c>exp = min(MaxBackoffMs, BaseBackoffMs * 2^(attemptNumber-1))</c>.</item>
    ///   <item>Apply jitter from <c>IRandomProvider.NextDouble(seed)</c> where
    ///         <c>seed = "{effectId:N}:retry:{attemptNumber}"</c>, scaled to
    ///         <c>[1.0, 1.25]</c>.</item>
    ///   <item>Re-cap at <c>MaxBackoffMs</c>.</item>
    /// </list></para>
    /// </summary>
    public static int ComputeBackoffMs(
        OutboundEffectOptions options,
        int attemptNumber,
        Guid effectId,
        TimeSpan? providerRetryAfter,
        IRandomProvider randomProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(randomProvider);

        if (providerRetryAfter is { } ra)
        {
            return Math.Min(options.MaxBackoffMs, (int)ra.TotalMilliseconds);
        }

        var shift = Math.Min(Math.Max(attemptNumber - 1, 0), 16);
        var exp = Math.Min(options.MaxBackoffMs, options.BaseBackoffMs * (1L << shift));
        var jitterSeed = BackoffSeed(effectId, attemptNumber);
        var jitterFactor = randomProvider.NextDouble(jitterSeed) * 0.25;
        return (int)Math.Min(options.MaxBackoffMs, exp * (1.0 + jitterFactor));
    }

    /// <summary>
    /// Canonical seed string consumed by <see cref="ComputeBackoffMs"/>.
    /// Exposed for test symmetry: reproducing the seed is required to assert
    /// that two runs produce identical backoff values.
    /// </summary>
    public static string BackoffSeed(Guid effectId, int attemptNumber) =>
        $"{effectId:N}:retry:{attemptNumber}";

    /// <summary>
    /// R3.B.3 / R-OUT-EFF-AMBIGUOUS-ATMOSTONCE-01 — the adapter shapes for
    /// which an Ambiguous dispatch classification MUST route to
    /// <c>ReconciliationRequired</c> instead of retrying. Retrying would risk
    /// a duplicate business outcome the provider cannot collapse, so the
    /// correct response is to surface the state ambiguity for resolution.
    /// </summary>
    public static bool IsAmbiguousUnsafeToRetry(OutboundIdempotencyShape shape) =>
        shape is OutboundIdempotencyShape.AtMostOnceRequired
            or OutboundIdempotencyShape.CompensatableOnly;

    private static string OutcomeTag(OutboundAdapterResult result) => result switch
    {
        OutboundAdapterResult.Acknowledged => "acknowledged",
        OutboundAdapterResult.DispatchedWithoutProviderOperationId => "dispatched_without_opid",
        OutboundAdapterResult.FinalizedSuccess => "finalized_success",
        OutboundAdapterResult.FinalizedFailure => "finalized_business_failed",
        OutboundAdapterResult.ReconciliationRequired => "reconciliation_required",
        OutboundAdapterResult.DispatchFailedPreAcceptance => "dispatch_failed_pre_acceptance",
        _ => "unknown",
    };
}
