using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Integration.OutboundEffect;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Projections.Integration.OutboundEffect;

namespace Whycespace.Projections.Integration.OutboundEffect;

/// <summary>
/// R3.B.1 — materializes the outbound-effect operator read model from the
/// eleven lifecycle event schemas. Idempotent via <c>LastEventId</c>; replay
/// of the same event id is a no-op (phase1-gate-projection-hardening pattern).
/// </summary>
public sealed class OutboundEffectProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<OutboundEffectScheduledEventSchema>,
    IProjectionHandler<OutboundEffectDispatchedEventSchema>,
    IProjectionHandler<OutboundEffectAcknowledgedEventSchema>,
    IProjectionHandler<OutboundEffectDispatchFailedEventSchema>,
    IProjectionHandler<OutboundEffectRetryAttemptedEventSchema>,
    IProjectionHandler<OutboundEffectRetryExhaustedEventSchema>,
    IProjectionHandler<OutboundEffectFinalizedEventSchema>,
    IProjectionHandler<OutboundEffectReconciliationRequiredEventSchema>,
    IProjectionHandler<OutboundEffectReconciledEventSchema>,
    IProjectionHandler<OutboundEffectCompensationRequestedEventSchema>,
    IProjectionHandler<OutboundEffectCancelledEventSchema>
{
    private readonly IOutboundEffectProjectionStore _store;
    private Guid _currentEventId = Guid.Empty;
    private DateTimeOffset? _currentTimestamp;

    public OutboundEffectProjectionHandler(IOutboundEffectProjectionStore store)
    {
        _store = store;
    }

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        _currentEventId = envelope.EventId;
        _currentTimestamp = envelope.Timestamp;
        return envelope.Payload switch
        {
            OutboundEffectScheduledEventSchema e => HandleAsync(e, cancellationToken),
            OutboundEffectDispatchedEventSchema e => HandleAsync(e, cancellationToken),
            OutboundEffectAcknowledgedEventSchema e => HandleAsync(e, cancellationToken),
            OutboundEffectDispatchFailedEventSchema e => HandleAsync(e, cancellationToken),
            OutboundEffectRetryAttemptedEventSchema e => HandleAsync(e, cancellationToken),
            OutboundEffectRetryExhaustedEventSchema e => HandleAsync(e, cancellationToken),
            OutboundEffectFinalizedEventSchema e => HandleAsync(e, cancellationToken),
            OutboundEffectReconciliationRequiredEventSchema e => HandleAsync(e, cancellationToken),
            OutboundEffectReconciledEventSchema e => HandleAsync(e, cancellationToken),
            OutboundEffectCompensationRequestedEventSchema e => HandleAsync(e, cancellationToken),
            OutboundEffectCancelledEventSchema e => HandleAsync(e, cancellationToken),
            _ => throw new InvalidOperationException(
                $"OutboundEffectProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}."),
        };
    }

    public async Task HandleAsync(OutboundEffectScheduledEventSchema e, CancellationToken ct = default)
    {
        var existing = await _store.GetAsync(e.AggregateId);
        if (existing is not null && existing.LastEventId == _currentEventId) return;

        await _store.UpsertAsync(new OutboundEffectReadModel
        {
            EffectId = e.AggregateId,
            ProviderId = e.ProviderId,
            EffectType = e.EffectType,
            IdempotencyKey = e.IdempotencyKey,
            Status = "Scheduled",
            AttemptCount = 0,
            MaxAttempts = e.MaxAttempts,
            LastTransitionAt = _currentTimestamp,
            LastEventId = _currentEventId,
        });
    }

    public Task HandleAsync(OutboundEffectDispatchedEventSchema e, CancellationToken ct = default)
        => Transition(e.AggregateId, model => model with
        {
            Status = "Dispatched",
            AttemptCount = e.AttemptNumber,
            LastTransitionAt = _currentTimestamp,
            LastEventId = _currentEventId,
        });

    public Task HandleAsync(OutboundEffectAcknowledgedEventSchema e, CancellationToken ct = default)
        => Transition(e.AggregateId, model => model with
        {
            Status = "Acknowledged",
            ProviderOperationId = e.ProviderOperationId,
            LastTransitionAt = _currentTimestamp,
            LastEventId = _currentEventId,
        });

    public Task HandleAsync(OutboundEffectDispatchFailedEventSchema e, CancellationToken ct = default)
        => Transition(e.AggregateId, model => model with
        {
            Status = e.Classification == "Terminal" ? "RetryExhausted" : "TransientFailed",
            AttemptCount = e.AttemptNumber,
            FailureClassification = e.Classification,
            FailureReason = e.Reason,
            LastTransitionAt = _currentTimestamp,
            LastEventId = _currentEventId,
        });

    public Task HandleAsync(OutboundEffectRetryAttemptedEventSchema e, CancellationToken ct = default)
        => Transition(e.AggregateId, model => model with
        {
            Status = "Scheduled",
            AttemptCount = e.AttemptNumber,
            LastTransitionAt = _currentTimestamp,
            LastEventId = _currentEventId,
        });

    public Task HandleAsync(OutboundEffectRetryExhaustedEventSchema e, CancellationToken ct = default)
        => Transition(e.AggregateId, model => model with
        {
            Status = "RetryExhausted",
            AttemptCount = e.TotalAttempts,
            FailureClassification = e.FinalClassification,
            LastTransitionAt = _currentTimestamp,
            LastEventId = _currentEventId,
        });

    public Task HandleAsync(OutboundEffectFinalizedEventSchema e, CancellationToken ct = default)
        => Transition(e.AggregateId, model => model with
        {
            Status = "Finalized",
            FinalityOutcome = e.FinalityOutcome,
            LastFinalitySource = e.FinalitySource,
            LastFinalityEvidenceDigest = e.FinalityEvidenceDigest,
            AckDeadline = null,
            FinalityDeadline = null,
            LastTransitionAt = _currentTimestamp,
            LastEventId = _currentEventId,
        });

    public Task HandleAsync(OutboundEffectReconciliationRequiredEventSchema e, CancellationToken ct = default)
        => Transition(e.AggregateId, model => model with
        {
            Status = "ReconciliationRequired",
            ReconciliationCause = e.Cause,
            LastTransitionAt = _currentTimestamp,
            LastEventId = _currentEventId,
        });

    public Task HandleAsync(OutboundEffectReconciledEventSchema e, CancellationToken ct = default)
        => Transition(e.AggregateId, model => model with
        {
            Status = "Reconciled",
            FinalityOutcome = e.Outcome,
            LastFinalityEvidenceDigest = e.EvidenceDigest,
            ReconcilerActorId = e.ReconcilerActorId,
            LastTransitionAt = _currentTimestamp,
            LastEventId = _currentEventId,
        });

    public Task HandleAsync(OutboundEffectCompensationRequestedEventSchema e, CancellationToken ct = default)
        => Transition(e.AggregateId, model => model with
        {
            Status = "CompensationRequested",
            LastTransitionAt = _currentTimestamp,
            LastEventId = _currentEventId,
        });

    public Task HandleAsync(OutboundEffectCancelledEventSchema e, CancellationToken ct = default)
        => Transition(e.AggregateId, model => model with
        {
            Status = "Cancelled",
            LastTransitionAt = _currentTimestamp,
            LastEventId = _currentEventId,
        });

    private async Task Transition(Guid effectId, Func<OutboundEffectReadModel, OutboundEffectReadModel> mutate)
    {
        var existing = await _store.GetAsync(effectId);
        if (existing is null) return;
        if (existing.LastEventId == _currentEventId) return;
        await _store.UpsertAsync(mutate(existing));
    }
}
