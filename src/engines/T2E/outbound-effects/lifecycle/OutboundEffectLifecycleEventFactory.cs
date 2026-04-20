using Whycespace.Domain.IntegrationSystem.OutboundEffect;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;

namespace Whycespace.Engines.T2E.OutboundEffects.Lifecycle;

/// <summary>
/// R3.B.1 / D-R3B-3 — T2E-tier factory owning canonical construction of every
/// <c>OutboundEffect*Event</c>. Mirrors <c>WorkflowLifecycleEventFactory</c>
/// discipline: the factory constructs events, the runtime persist pipeline
/// lands them, the aggregate <c>Apply</c> reconstructs state on replay. No
/// aggregate mutation paths exist.
///
/// <para>R-OUT-EFF-SEAM-03 — all lifecycle events MUST be constructed here;
/// direct <c>new OutboundEffect*Event(...)</c> in dispatcher / relay /
/// projection / adapters is a guard violation.</para>
/// </summary>
public sealed class OutboundEffectLifecycleEventFactory
{
    private readonly IPayloadTypeRegistry _payloadTypes;

    public OutboundEffectLifecycleEventFactory(IPayloadTypeRegistry payloadTypes)
    {
        ArgumentNullException.ThrowIfNull(payloadTypes);
        _payloadTypes = payloadTypes;
    }

    /// <summary>R3.B.1 / R-OUT-EFF-SEAM-01 — canonical construction of the aggregate itself.</summary>
    public OutboundEffectAggregate Start(
        Guid effectId,
        string providerId,
        string effectType,
        OutboundIdempotencyKey idempotencyKey,
        object payload,
        string schedulerActorId,
        OutboundEffectOptions options)
    {
        ArgumentNullException.ThrowIfNull(idempotencyKey);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(options);

        return OutboundEffectAggregate.Start(
            effectId,
            providerId,
            effectType,
            idempotencyKey.Value,
            ResolvePayloadTypeName(payload),
            schedulerActorId,
            options.DispatchTimeoutMs,
            options.TotalBudgetMs,
            options.AckTimeoutMs,
            options.FinalityWindowMs,
            options.MaxAttempts,
            payload);
    }

    public OutboundEffectDispatchedEvent Dispatched(
        Guid effectId,
        int attemptNumber,
        DateTimeOffset dispatchStartedAt,
        DateTimeOffset dispatchCompletedAt,
        string? transportEvidenceDigest = null)
        => new(
            new AggregateId(effectId),
            attemptNumber,
            dispatchStartedAt,
            dispatchCompletedAt,
            transportEvidenceDigest);

    public OutboundEffectAcknowledgedEvent Acknowledged(
        Guid effectId,
        ProviderOperationIdentity providerOperation,
        string? ackPayloadDigest = null)
    {
        ArgumentNullException.ThrowIfNull(providerOperation);
        if (string.IsNullOrWhiteSpace(providerOperation.ProviderOperationId))
            throw new ArgumentException(
                OutboundEffectErrors.AcknowledgedRequiresProviderOperationId, nameof(providerOperation));

        return new OutboundEffectAcknowledgedEvent(
            new AggregateId(effectId),
            providerOperation.ProviderId,
            providerOperation.ProviderOperationId,
            providerOperation.IdempotencyKeyUsed,
            ackPayloadDigest);
    }

    public OutboundEffectDispatchFailedEvent DispatchFailed(
        Guid effectId,
        int attemptNumber,
        OutboundAdapterClassification classification,
        string reason,
        int? retryAfterMs = null)
        => new(
            new AggregateId(effectId),
            attemptNumber,
            classification.ToString(),
            reason,
            retryAfterMs);

    public OutboundEffectRetryAttemptedEvent RetryAttempted(
        Guid effectId,
        int attemptNumber,
        DateTimeOffset nextAttemptAt,
        int backoffMs,
        OutboundAdapterClassification precedingClassification)
        => new(
            new AggregateId(effectId),
            attemptNumber,
            nextAttemptAt,
            backoffMs,
            precedingClassification.ToString());

    public OutboundEffectRetryExhaustedEvent RetryExhausted(
        Guid effectId,
        int totalAttempts,
        OutboundAdapterClassification finalClassification)
        => new(
            new AggregateId(effectId),
            totalAttempts,
            finalClassification.ToString());

    public OutboundEffectFinalizedEvent Finalized(
        Guid effectId,
        OutboundFinalityOutcome outcome,
        string evidenceDigest,
        DateTimeOffset finalizedAt,
        string finalitySource)
        => new(
            new AggregateId(effectId),
            outcome.ToString(),
            evidenceDigest,
            finalizedAt,
            finalitySource);

    public OutboundEffectReconciliationRequiredEvent ReconciliationRequired(
        Guid effectId,
        OutboundReconciliationCause cause,
        DateTimeOffset observedAt)
        => new(
            new AggregateId(effectId),
            cause.ToString(),
            observedAt);

    public OutboundEffectReconciledEvent Reconciled(
        Guid effectId,
        OutboundFinalityOutcome outcome,
        string evidenceDigest,
        string reconcilerActorId)
        => new(
            new AggregateId(effectId),
            outcome.ToString(),
            evidenceDigest,
            reconcilerActorId);

    public OutboundEffectCompensationRequestedEvent CompensationRequested(
        Guid effectId,
        OutboundFinalityOutcome triggeringOutcome,
        string? ownerAggregateType = null,
        Guid? ownerAggregateId = null)
        => new(
            new AggregateId(effectId),
            triggeringOutcome.ToString(),
            ownerAggregateType,
            ownerAggregateId);

    public OutboundEffectCancelledEvent Cancelled(
        Guid effectId,
        string cancellationReason,
        bool preDispatch = true)
        => new(
            new AggregateId(effectId),
            cancellationReason,
            preDispatch);

    private string? ResolvePayloadTypeName(object? value)
    {
        if (value is null) return null;
        return _payloadTypes.TryGetName(value.GetType(), out var name) ? name : null;
    }
}
