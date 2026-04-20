namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 / R-OUT-EFF-SEAM-01 — the ONLY way to introduce a new outbound
/// intent into the runtime. Emits <c>OutboundEffectScheduledEvent</c> on a
/// fresh <c>OutboundEffectAggregate</c>; the concurrent
/// <c>OutboundEffectRelay</c> picks up the queue row and invokes the
/// registered adapter.
///
/// <para>Callable from engines and runtime. Domain may NOT call this directly
/// (<c>R-OUT-EFF-PROHIBITION-04</c>) — scheduling happens through engines or
/// the runtime control plane.</para>
/// </summary>
public interface IOutboundEffectDispatcher
{
    /// <summary>
    /// Schedules an outbound effect. Duplicate <paramref name="intent"/>
    /// <see cref="OutboundEffectIntent.IdempotencyKey"/> returns the existing
    /// effect id with <c>DedupHit = true</c> — no second aggregate is created.
    /// </summary>
    Task<OutboundEffectScheduleResult> ScheduleAsync(
        OutboundEffectIntent intent,
        CommandContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pre-dispatch cancellation. No-op (returns false) if the aggregate has
    /// already transitioned past <c>Scheduled</c>.
    /// </summary>
    Task<bool> CancelScheduledAsync(
        Guid effectId,
        string cancellationReason,
        CommandContext context,
        CancellationToken cancellationToken = default);
}
