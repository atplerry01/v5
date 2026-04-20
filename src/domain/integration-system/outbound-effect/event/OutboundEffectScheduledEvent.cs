using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 — lifecycle event: durable intent persisted. Emitted from
/// <c>OutboundEffectAggregate.Start(...)</c> via
/// <c>OutboundEffectLifecycleEventFactory</c>. Its persistence is the
/// "durable-intent-first" moment required by R-OUT-EFF-OUTBOX-01; no adapter
/// may be invoked before this event lands on the event store.
/// </summary>
public sealed record OutboundEffectScheduledEvent(
    AggregateId AggregateId,
    string ProviderId,
    string EffectType,
    string IdempotencyKey,
    string? PayloadTypeDiscriminator,
    string SchedulerActorId,
    int DispatchTimeoutMs,
    int TotalBudgetMs,
    int AckTimeoutMs,
    int FinalityWindowMs,
    int MaxAttempts,
    object? Payload = null) : DomainEvent;
