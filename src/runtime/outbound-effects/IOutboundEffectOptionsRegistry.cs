using Whycespace.Shared.Contracts.Runtime.OutboundEffects;

namespace Whycespace.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 / R-OUT-EFF-TIMEOUT-02 — per-provider options lookup. Composition
/// registers one <see cref="OutboundEffectOptions"/> per sanctioned provider;
/// the dispatcher consults this before scheduling to refuse unconfigured
/// providers with <c>OutboundEffectErrors.OptionsMissing</c>.
/// </summary>
public interface IOutboundEffectOptionsRegistry
{
    bool TryGet(string providerId, out OutboundEffectOptions? options);
}

/// <summary>Dictionary-backed registry populated by composition.</summary>
public sealed class OutboundEffectOptionsRegistry : IOutboundEffectOptionsRegistry
{
    private readonly IReadOnlyDictionary<string, OutboundEffectOptions> _byProvider;

    public OutboundEffectOptionsRegistry(IEnumerable<OutboundEffectOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _byProvider = options.ToDictionary(o => o.ProviderId, StringComparer.Ordinal);
    }

    public bool TryGet(string providerId, out OutboundEffectOptions? options)
    {
        if (_byProvider.TryGetValue(providerId, out var match))
        {
            options = match;
            return true;
        }
        options = null;
        return false;
    }
}
