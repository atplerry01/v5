namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — caller-supplied intent to schedule an outbound effect. Validation
/// occurs in <see cref="IOutboundEffectDispatcher.ScheduleAsync"/> (provider
/// registered, payload registered, idempotency key non-null).
///
/// Override fields are optional per-intent deviations from the per-provider
/// <see cref="OutboundEffectOptions"/>; they MUST be null in replay contexts
/// where determinism matters (overrides are applied at schedule time only).
/// </summary>
public sealed record OutboundEffectIntent(
    string ProviderId,
    string EffectType,
    OutboundIdempotencyKey IdempotencyKey,
    object Payload,
    Guid? CorrelationId = null,
    Guid? CausationId = null,
    TimeSpan? DispatchTimeoutOverride = null,
    TimeSpan? AckTimeoutOverride = null,
    TimeSpan? FinalityWindowOverride = null,
    int? MaxAttemptsOverride = null);
