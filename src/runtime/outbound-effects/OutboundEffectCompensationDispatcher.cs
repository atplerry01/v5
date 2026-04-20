using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;

namespace Whycespace.Runtime.OutboundEffects;

/// <summary>
/// R3.B.5 — best-effort fan-out of <see cref="OutboundEffectCompensationSignal"/>
/// to registered <see cref="IOutboundEffectCompensationHandler"/> instances.
/// Invoked by <see cref="OutboundEffectFinalityService"/> and
/// <see cref="OutboundEffectRelay"/> AFTER the lifecycle event has been
/// persisted and anchored — the compensation signal is durable irrespective
/// of in-process handler outcome.
///
/// <para><b>Handler-failure discipline:</b> each handler runs in its own
/// try/catch. Exceptions are logged and counted via
/// <c>outbound.effect.compensation.handler_failed</c>; the next handler
/// still runs.</para>
///
/// <para><b>Missing-handler discipline</b> (R-OUT-EFF-COMPENSATION-UNHANDLED-01):
/// when the registry returns zero handlers for the effect type, the
/// dispatcher increments <c>outbound.effect.compensation.unhandled</c> and
/// logs at WARN level so the condition is visible to operators. The
/// compensation event remains on the aggregate stream for later consumption —
/// later handler registration + replay will eventually deliver it.</para>
/// </summary>
public sealed class OutboundEffectCompensationDispatcher
{
    private readonly IOutboundEffectCompensationHandlerRegistry _registry;
    private readonly OutboundEffectsMeter _meter;
    private readonly ILogger<OutboundEffectCompensationDispatcher>? _logger;

    public OutboundEffectCompensationDispatcher(
        IOutboundEffectCompensationHandlerRegistry registry,
        OutboundEffectsMeter meter,
        ILogger<OutboundEffectCompensationDispatcher>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(meter);
        _registry = registry;
        _meter = meter;
        _logger = logger;
    }

    public async Task DispatchAsync(
        OutboundEffectCompensationSignal signal,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(signal);

        _meter.CompensationEmitted.Add(1,
            new KeyValuePair<string, object?>("provider", signal.ProviderId),
            new KeyValuePair<string, object?>("effect_type", signal.EffectType));

        var handlers = _registry.ResolveHandlers(signal.EffectType);
        if (handlers.Count == 0)
        {
            _meter.CompensationUnhandled.Add(1,
                new KeyValuePair<string, object?>("provider", signal.ProviderId),
                new KeyValuePair<string, object?>("effect_type", signal.EffectType));
            _logger?.LogWarning(
                "Outbound effect compensation requested but NO handler is registered. " +
                "effectId={EffectId} effectType={EffectType} providerId={ProviderId} triggeringOutcome={Outcome}. " +
                "The compensation event remains on the aggregate stream; register an " +
                "IOutboundEffectCompensationHandler for this effect type to consume it.",
                signal.EffectId, signal.EffectType, signal.ProviderId, signal.TriggeringOutcome);
            return;
        }

        foreach (var handler in handlers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await handler.HandleAsync(signal, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _meter.CompensationHandlerFailed.Add(1,
                    new KeyValuePair<string, object?>("provider", signal.ProviderId),
                    new KeyValuePair<string, object?>("effect_type", signal.EffectType),
                    new KeyValuePair<string, object?>("handler", handler.GetType().Name));
                _logger?.LogError(ex,
                    "Outbound effect compensation handler '{HandlerType}' threw for effectId={EffectId}. " +
                    "Subsequent handlers still run; the compensation event remains on the aggregate stream.",
                    handler.GetType().Name, signal.EffectId);
            }
        }
    }
}
