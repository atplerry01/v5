using Whycespace.Shared.Contracts.Events.Integration.OutboundEffect;
using DomainEvents = Whycespace.Domain.IntegrationSystem.OutboundEffect;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// R3.B.1 — schema module for <c>integration-system/outbound-effect</c>. Binds
/// the eleven outbound-effect lifecycle domain events to their wire schemas,
/// and registers payload mappers for outbox publication. Mirrors
/// <see cref="WorkflowExecutionSchemaModule"/> pattern exactly.
/// </summary>
public sealed class OutboundEffectSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "OutboundEffectScheduledEvent",
            EventVersion.Default,
            typeof(DomainEvents.OutboundEffectScheduledEvent),
            typeof(OutboundEffectScheduledEventSchema));
        sink.RegisterSchema(
            "OutboundEffectDispatchedEvent",
            EventVersion.Default,
            typeof(DomainEvents.OutboundEffectDispatchedEvent),
            typeof(OutboundEffectDispatchedEventSchema));
        sink.RegisterSchema(
            "OutboundEffectAcknowledgedEvent",
            EventVersion.Default,
            typeof(DomainEvents.OutboundEffectAcknowledgedEvent),
            typeof(OutboundEffectAcknowledgedEventSchema));
        sink.RegisterSchema(
            "OutboundEffectDispatchFailedEvent",
            EventVersion.Default,
            typeof(DomainEvents.OutboundEffectDispatchFailedEvent),
            typeof(OutboundEffectDispatchFailedEventSchema));
        sink.RegisterSchema(
            "OutboundEffectRetryAttemptedEvent",
            EventVersion.Default,
            typeof(DomainEvents.OutboundEffectRetryAttemptedEvent),
            typeof(OutboundEffectRetryAttemptedEventSchema));
        sink.RegisterSchema(
            "OutboundEffectRetryExhaustedEvent",
            EventVersion.Default,
            typeof(DomainEvents.OutboundEffectRetryExhaustedEvent),
            typeof(OutboundEffectRetryExhaustedEventSchema));
        sink.RegisterSchema(
            "OutboundEffectFinalizedEvent",
            EventVersion.Default,
            typeof(DomainEvents.OutboundEffectFinalizedEvent),
            typeof(OutboundEffectFinalizedEventSchema));
        sink.RegisterSchema(
            "OutboundEffectReconciliationRequiredEvent",
            EventVersion.Default,
            typeof(DomainEvents.OutboundEffectReconciliationRequiredEvent),
            typeof(OutboundEffectReconciliationRequiredEventSchema));
        sink.RegisterSchema(
            "OutboundEffectReconciledEvent",
            EventVersion.Default,
            typeof(DomainEvents.OutboundEffectReconciledEvent),
            typeof(OutboundEffectReconciledEventSchema));
        sink.RegisterSchema(
            "OutboundEffectCompensationRequestedEvent",
            EventVersion.Default,
            typeof(DomainEvents.OutboundEffectCompensationRequestedEvent),
            typeof(OutboundEffectCompensationRequestedEventSchema));
        sink.RegisterSchema(
            "OutboundEffectCancelledEvent",
            EventVersion.Default,
            typeof(DomainEvents.OutboundEffectCancelledEvent),
            typeof(OutboundEffectCancelledEventSchema));

        sink.RegisterPayloadMapper("OutboundEffectScheduledEvent", e =>
        {
            var evt = (DomainEvents.OutboundEffectScheduledEvent)e;
            return new OutboundEffectScheduledEventSchema(
                evt.AggregateId.Value,
                evt.ProviderId,
                evt.EffectType,
                evt.IdempotencyKey,
                evt.PayloadTypeDiscriminator,
                evt.SchedulerActorId,
                evt.DispatchTimeoutMs,
                evt.TotalBudgetMs,
                evt.AckTimeoutMs,
                evt.FinalityWindowMs,
                evt.MaxAttempts);
        });
        sink.RegisterPayloadMapper("OutboundEffectDispatchedEvent", e =>
        {
            var evt = (DomainEvents.OutboundEffectDispatchedEvent)e;
            return new OutboundEffectDispatchedEventSchema(
                evt.AggregateId.Value,
                evt.AttemptNumber,
                evt.DispatchStartedAt,
                evt.DispatchCompletedAt,
                evt.TransportEvidenceDigest);
        });
        sink.RegisterPayloadMapper("OutboundEffectAcknowledgedEvent", e =>
        {
            var evt = (DomainEvents.OutboundEffectAcknowledgedEvent)e;
            return new OutboundEffectAcknowledgedEventSchema(
                evt.AggregateId.Value,
                evt.ProviderId,
                evt.ProviderOperationId,
                evt.IdempotencyKeyUsed,
                evt.AckPayloadDigest);
        });
        sink.RegisterPayloadMapper("OutboundEffectDispatchFailedEvent", e =>
        {
            var evt = (DomainEvents.OutboundEffectDispatchFailedEvent)e;
            return new OutboundEffectDispatchFailedEventSchema(
                evt.AggregateId.Value,
                evt.AttemptNumber,
                evt.Classification,
                evt.Reason,
                evt.RetryAfterMs);
        });
        sink.RegisterPayloadMapper("OutboundEffectRetryAttemptedEvent", e =>
        {
            var evt = (DomainEvents.OutboundEffectRetryAttemptedEvent)e;
            return new OutboundEffectRetryAttemptedEventSchema(
                evt.AggregateId.Value,
                evt.AttemptNumber,
                evt.NextAttemptAt,
                evt.BackoffMs,
                evt.PrecedingClassification);
        });
        sink.RegisterPayloadMapper("OutboundEffectRetryExhaustedEvent", e =>
        {
            var evt = (DomainEvents.OutboundEffectRetryExhaustedEvent)e;
            return new OutboundEffectRetryExhaustedEventSchema(
                evt.AggregateId.Value,
                evt.TotalAttempts,
                evt.FinalClassification);
        });
        sink.RegisterPayloadMapper("OutboundEffectFinalizedEvent", e =>
        {
            var evt = (DomainEvents.OutboundEffectFinalizedEvent)e;
            return new OutboundEffectFinalizedEventSchema(
                evt.AggregateId.Value,
                evt.FinalityOutcome,
                evt.FinalityEvidenceDigest,
                evt.FinalizedAt,
                evt.FinalitySource);
        });
        sink.RegisterPayloadMapper("OutboundEffectReconciliationRequiredEvent", e =>
        {
            var evt = (DomainEvents.OutboundEffectReconciliationRequiredEvent)e;
            return new OutboundEffectReconciliationRequiredEventSchema(
                evt.AggregateId.Value,
                evt.Cause,
                evt.ObservedAt);
        });
        sink.RegisterPayloadMapper("OutboundEffectReconciledEvent", e =>
        {
            var evt = (DomainEvents.OutboundEffectReconciledEvent)e;
            return new OutboundEffectReconciledEventSchema(
                evt.AggregateId.Value,
                evt.Outcome,
                evt.EvidenceDigest,
                evt.ReconcilerActorId);
        });
        sink.RegisterPayloadMapper("OutboundEffectCompensationRequestedEvent", e =>
        {
            var evt = (DomainEvents.OutboundEffectCompensationRequestedEvent)e;
            return new OutboundEffectCompensationRequestedEventSchema(
                evt.AggregateId.Value,
                evt.TriggeringOutcome,
                evt.OwnerAggregateType,
                evt.OwnerAggregateId);
        });
        sink.RegisterPayloadMapper("OutboundEffectCancelledEvent", e =>
        {
            var evt = (DomainEvents.OutboundEffectCancelledEvent)e;
            return new OutboundEffectCancelledEventSchema(
                evt.AggregateId.Value,
                evt.CancellationReason,
                evt.PreDispatch);
        });
    }
}
