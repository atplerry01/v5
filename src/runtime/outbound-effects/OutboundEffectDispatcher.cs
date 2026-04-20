using System.Diagnostics;
using Whycespace.Engines.T2E.OutboundEffects.Lifecycle;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 / R-OUT-EFF-SEAM-01 — the sole seam for scheduling outbound effects.
/// Creates a fresh <c>OutboundEffectAggregate</c> via
/// <see cref="OutboundEffectLifecycleEventFactory"/>, persists the
/// <c>OutboundEffectScheduledEvent</c> through the canonical event fabric
/// (persist → chain → outbox), and inserts the queue row atomically so the
/// concurrent relay can pick up the durable intent.
///
/// <para>Duplicate <c>(ProviderId, IdempotencyKey)</c> short-circuits to the
/// existing effect id with <c>DedupHit = true</c> (R-OUT-EFF-IDEM-02).</para>
/// </summary>
public sealed class OutboundEffectDispatcher : IOutboundEffectDispatcher
{
    private readonly IEventFabric _eventFabric;
    private readonly IOutboundEffectQueueStore _queueStore;
    private readonly IOutboundEffectAdapterRegistry _adapterRegistry;
    private readonly IOutboundEffectOptionsRegistry _optionsRegistry;
    private readonly OutboundEffectLifecycleEventFactory _lifecycleFactory;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly OutboundEffectsMeter _meter;

    public OutboundEffectDispatcher(
        IEventFabric eventFabric,
        IOutboundEffectQueueStore queueStore,
        IOutboundEffectAdapterRegistry adapterRegistry,
        IOutboundEffectOptionsRegistry optionsRegistry,
        OutboundEffectLifecycleEventFactory lifecycleFactory,
        IIdGenerator idGenerator,
        IClock clock,
        OutboundEffectsMeter meter)
    {
        _eventFabric = eventFabric;
        _queueStore = queueStore;
        _adapterRegistry = adapterRegistry;
        _optionsRegistry = optionsRegistry;
        _lifecycleFactory = lifecycleFactory;
        _idGenerator = idGenerator;
        _clock = clock;
        _meter = meter;
    }

    public async Task<OutboundEffectScheduleResult> ScheduleAsync(
        OutboundEffectIntent intent,
        CommandContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(intent.ProviderId))
            throw new ArgumentException(OutboundEffectErrors.ProviderIdRequired, nameof(intent));
        if (string.IsNullOrWhiteSpace(intent.EffectType))
            throw new ArgumentException(OutboundEffectErrors.EffectTypeRequired, nameof(intent));
        ArgumentNullException.ThrowIfNull(intent.Payload);

        // R5.A Phase 2 / R-TRACE-OUTBOUND-SCHEDULE-SPAN-01 — canonical span
        // around the whole schedule path so operators see dedup-hits and
        // new schedules as distinct span outcomes. Started AFTER arg
        // validation so malformed-intent failures stay on the caller's span.
        using var activity = WhyceActivitySources.OutboundEffects.StartActivity(
            WhyceActivitySources.Spans.OutboundEffectSchedule,
            ActivityKind.Internal);
        activity?.SetTag(WhyceActivitySources.Attributes.ProviderId, intent.ProviderId);
        activity?.SetTag(WhyceActivitySources.Attributes.EffectType, intent.EffectType);
        activity?.SetTag(WhyceActivitySources.Attributes.IdempotencyKey, intent.IdempotencyKey.Value);
        activity?.SetTag(WhyceActivitySources.Attributes.CorrelationId, context.CorrelationId);

        if (!_adapterRegistry.TryGet(intent.ProviderId, out _))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "provider_not_registered");
            throw new InvalidOperationException(OutboundEffectErrors.ProviderNotRegistered);
        }

        if (!_optionsRegistry.TryGet(intent.ProviderId, out var baseOptions) || baseOptions is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "options_missing");
            throw new InvalidOperationException(OutboundEffectErrors.OptionsMissing);
        }

        var existing = await _queueStore.FindByIdempotencyKeyAsync(
            intent.ProviderId, intent.IdempotencyKey.Value, cancellationToken);
        if (existing is { } existingId)
        {
            _meter.ScheduleDedupHit.Add(1,
                new KeyValuePair<string, object?>("provider", intent.ProviderId),
                new KeyValuePair<string, object?>("effect_type", intent.EffectType));
            activity?.SetTag(WhyceActivitySources.Attributes.DedupHit, true);
            activity?.SetTag(WhyceActivitySources.Attributes.Outcome, "dedup_hit");
            activity?.SetTag(WhyceActivitySources.Attributes.TargetId, existingId);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return new OutboundEffectScheduleResult(existingId, DedupHit: true);
        }

        var options = ApplyOverrides(baseOptions, intent);

        var effectIdSeed = $"outbound:{intent.ProviderId}:{intent.IdempotencyKey.Value}";
        var effectId = _idGenerator.Generate(effectIdSeed);

        var aggregate = _lifecycleFactory.Start(
            effectId,
            intent.ProviderId,
            intent.EffectType,
            intent.IdempotencyKey,
            intent.Payload,
            context.ActorId,
            options);

        var now = _clock.UtcNow;
        var queueEntry = new OutboundEffectQueueEntry
        {
            EffectId = effectId,
            ProviderId = intent.ProviderId,
            EffectType = intent.EffectType,
            IdempotencyKey = intent.IdempotencyKey.Value,
            Status = OutboundEffectQueueStatus.Scheduled,
            AttemptCount = 0,
            MaxAttempts = options.MaxAttempts,
            NextAttemptAt = now,
            DispatchDeadline = now.AddMilliseconds(options.TotalBudgetMs),
            AckDeadline = null,
            FinalityDeadline = null,
            LastError = null,
            ClaimedBy = null,
            ClaimedAt = null,
            CreatedAt = now,
            UpdatedAt = now,
            Payload = intent.Payload,
        };

        await _queueStore.InsertAsync(queueEntry, cancellationToken);

        var emission = new List<object>(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();

        var scheduleContext = RouteScheduledEventToIntegrationStream(context, effectId);
        await _eventFabric.ProcessAsync(emission, scheduleContext, cancellationToken);

        _meter.Scheduled.Add(1,
            new KeyValuePair<string, object?>("provider", intent.ProviderId),
            new KeyValuePair<string, object?>("effect_type", intent.EffectType));

        activity?.SetTag(WhyceActivitySources.Attributes.DedupHit, false);
        activity?.SetTag(WhyceActivitySources.Attributes.Outcome, "scheduled");
        activity?.SetTag(WhyceActivitySources.Attributes.TargetId, effectId);
        activity?.SetStatus(ActivityStatusCode.Ok);
        return new OutboundEffectScheduleResult(effectId, DedupHit: false);
    }

    public async Task<bool> CancelScheduledAsync(
        Guid effectId,
        string cancellationReason,
        CommandContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (string.IsNullOrWhiteSpace(cancellationReason))
            throw new ArgumentException("cancellationReason is required.", nameof(cancellationReason));

        var entry = await _queueStore.GetAsync(effectId, cancellationToken);
        if (entry is null) return false;
        if (entry.Status != OutboundEffectQueueStatus.Scheduled) return false;

        var cancelledEvent = _lifecycleFactory.Cancelled(effectId, cancellationReason, preDispatch: true);

        var cancelContext = RouteScheduledEventToIntegrationStream(context, effectId);
        await _eventFabric.ProcessAsync(new object[] { cancelledEvent }, cancelContext, cancellationToken);

        var now = _clock.UtcNow;
        await _queueStore.UpdateStatusAsync(
            effectId,
            OutboundEffectQueueStatus.Cancelled,
            entry.AttemptCount,
            nextAttemptAt: now,
            ackDeadline: entry.AckDeadline,
            finalityDeadline: entry.FinalityDeadline,
            lastError: null,
            updatedAt: now,
            cancellationToken);

        _meter.Cancelled.Add(1,
            new KeyValuePair<string, object?>("provider", entry.ProviderId),
            new KeyValuePair<string, object?>("effect_type", entry.EffectType));

        return true;
    }

    private static OutboundEffectOptions ApplyOverrides(OutboundEffectOptions baseOptions, OutboundEffectIntent intent)
    {
        var dispatch = intent.DispatchTimeoutOverride is { } d ? (int)d.TotalMilliseconds : baseOptions.DispatchTimeoutMs;
        var ack = intent.AckTimeoutOverride is { } a ? (int)a.TotalMilliseconds : baseOptions.AckTimeoutMs;
        var finality = intent.FinalityWindowOverride is { } f ? (int)f.TotalMilliseconds : baseOptions.FinalityWindowMs;
        var maxAttempts = intent.MaxAttemptsOverride ?? baseOptions.MaxAttempts;

        if (dispatch == baseOptions.DispatchTimeoutMs
            && ack == baseOptions.AckTimeoutMs
            && finality == baseOptions.FinalityWindowMs
            && maxAttempts == baseOptions.MaxAttempts)
        {
            return baseOptions;
        }

        return baseOptions with
        {
            DispatchTimeoutMs = dispatch,
            AckTimeoutMs = ack,
            FinalityWindowMs = finality,
            MaxAttempts = maxAttempts,
        };
    }

    private static CommandContext RouteScheduledEventToIntegrationStream(CommandContext context, Guid aggregateId)
    {
        return new CommandContext
        {
            CorrelationId = context.CorrelationId,
            CausationId = context.CausationId,
            CommandId = context.CommandId,
            TenantId = context.TenantId,
            ActorId = context.ActorId,
            AggregateId = aggregateId,
            PolicyId = context.PolicyId,
            IsSystem = context.IsSystem,
            Classification = "integration-system",
            Context = "outbound-effect",
            Domain = "outbound-effect",
        };
    }
}
