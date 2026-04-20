using Whycespace.Shared.Contracts.Runtime.OutboundEffects;

namespace Whycespace.Runtime.OutboundEffects;

/// <summary>
/// R3.B.5 — DI-populated <see cref="IOutboundEffectCompensationHandlerRegistry"/>.
/// Multiple handlers per effect type are permitted (fan-out); lookup is
/// O(1) on effect-type string.
/// </summary>
public sealed class OutboundEffectCompensationHandlerRegistry : IOutboundEffectCompensationHandlerRegistry
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<IOutboundEffectCompensationHandler>> _byEffectType;
    private static readonly IReadOnlyList<IOutboundEffectCompensationHandler> Empty = [];

    public OutboundEffectCompensationHandlerRegistry(
        IEnumerable<IOutboundEffectCompensationHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        _byEffectType = handlers
            .GroupBy(h => h.EffectType, StringComparer.Ordinal)
            .ToDictionary<IGrouping<string, IOutboundEffectCompensationHandler>,
                          string,
                          IReadOnlyList<IOutboundEffectCompensationHandler>>(
                g => g.Key,
                g => g.ToList(),
                StringComparer.Ordinal);
    }

    public IReadOnlyList<IOutboundEffectCompensationHandler> ResolveHandlers(string effectType) =>
        _byEffectType.TryGetValue(effectType, out var list) ? list : Empty;
}
