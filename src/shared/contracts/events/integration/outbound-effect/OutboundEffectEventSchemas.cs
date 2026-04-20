namespace Whycespace.Shared.Contracts.Events.Integration.OutboundEffect;

/// <summary>
/// R3.B.1 — wire schemas for the eleven <c>OutboundEffect*</c> lifecycle
/// events. One record per domain event; payload mappers in
/// <c>OutboundEffectSchemaModule</c> translate domain → schema for outbox
/// publication.
/// </summary>
public sealed record OutboundEffectScheduledEventSchema(
    Guid AggregateId,
    string ProviderId,
    string EffectType,
    string IdempotencyKey,
    string? PayloadTypeDiscriminator,
    string SchedulerActorId,
    int DispatchTimeoutMs,
    int TotalBudgetMs,
    int AckTimeoutMs,
    int FinalityWindowMs,
    int MaxAttempts);

public sealed record OutboundEffectDispatchedEventSchema(
    Guid AggregateId,
    int AttemptNumber,
    DateTimeOffset DispatchStartedAt,
    DateTimeOffset DispatchCompletedAt,
    string? TransportEvidenceDigest);

public sealed record OutboundEffectAcknowledgedEventSchema(
    Guid AggregateId,
    string ProviderId,
    string ProviderOperationId,
    string? IdempotencyKeyUsed,
    string? AckPayloadDigest);

public sealed record OutboundEffectDispatchFailedEventSchema(
    Guid AggregateId,
    int AttemptNumber,
    string Classification,
    string Reason,
    int? RetryAfterMs);

public sealed record OutboundEffectRetryAttemptedEventSchema(
    Guid AggregateId,
    int AttemptNumber,
    DateTimeOffset NextAttemptAt,
    int BackoffMs,
    string PrecedingClassification);

public sealed record OutboundEffectRetryExhaustedEventSchema(
    Guid AggregateId,
    int TotalAttempts,
    string FinalClassification);

public sealed record OutboundEffectFinalizedEventSchema(
    Guid AggregateId,
    string FinalityOutcome,
    string FinalityEvidenceDigest,
    DateTimeOffset FinalizedAt,
    string FinalitySource);

public sealed record OutboundEffectReconciliationRequiredEventSchema(
    Guid AggregateId,
    string Cause,
    DateTimeOffset ObservedAt);

public sealed record OutboundEffectReconciledEventSchema(
    Guid AggregateId,
    string Outcome,
    string EvidenceDigest,
    string ReconcilerActorId);

public sealed record OutboundEffectCompensationRequestedEventSchema(
    Guid AggregateId,
    string TriggeringOutcome,
    string? OwnerAggregateType,
    Guid? OwnerAggregateId);

public sealed record OutboundEffectCancelledEventSchema(
    Guid AggregateId,
    string CancellationReason,
    bool PreDispatch);
