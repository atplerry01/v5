namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.5 — caller-side compensation handler. The runtime emits
/// <c>OutboundEffectCompensationRequestedEvent</c> when a qualifying outcome
/// occurs; registered handlers receive a shared-contracts
/// <see cref="OutboundEffectCompensationSignal"/> projected from that event
/// and trigger their own compensation workflow (dispatch a caller-aggregate
/// command, send an operator alert, roll back local preparatory state, etc.).
///
/// <para>Compensation is <b>domain-level</b>, not runtime-level (parent
/// design §9.2). The runtime signals; the caller decides what to do.</para>
///
/// <para><b>Handler discipline:</b> runs inside the finality / relay call
/// path AFTER the lifecycle event has been persisted + anchored. A handler
/// failure MUST NOT roll back the emission — the compensation request is
/// durable, and the consumer is eventually consistent. The compensation
/// dispatcher catches handler exceptions and surfaces them via metric + log
/// without re-throwing.</para>
/// </summary>
public interface IOutboundEffectCompensationHandler
{
    /// <summary>
    /// Effect type this handler consumes. Matches
    /// <c>OutboundEffectScheduledEvent.EffectType</c>. Use a dotted naming
    /// convention (<c>"payment.settle"</c>, <c>"notification.send"</c>).
    /// </summary>
    string EffectType { get; }

    Task HandleAsync(
        OutboundEffectCompensationSignal signal,
        CancellationToken cancellationToken);
}

/// <summary>
/// R3.B.5 — registry of caller-side compensation handlers keyed by
/// <see cref="IOutboundEffectCompensationHandler.EffectType"/>.
/// </summary>
public interface IOutboundEffectCompensationHandlerRegistry
{
    IReadOnlyList<IOutboundEffectCompensationHandler> ResolveHandlers(string effectType);
}
