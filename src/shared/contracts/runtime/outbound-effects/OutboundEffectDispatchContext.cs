namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — passed to <see cref="IOutboundEffectAdapter.DispatchAsync"/> by
/// <c>OutboundEffectRelay</c>. Carries everything the adapter needs to produce
/// a deterministic provider call + per-attempt context for observability.
/// </summary>
public sealed record OutboundEffectDispatchContext(
    Guid EffectId,
    OutboundIdempotencyKey IdempotencyKey,
    int AttemptNumber,
    object Payload,
    Guid CorrelationId,
    Guid CausationId,
    string ActorId,
    TimeSpan DispatchTimeout);
