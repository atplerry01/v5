using Whycespace.Shared.Contracts.Runtime.OutboundEffects;

namespace Whycespace.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — DI-populated registry of adapters keyed by
/// <see cref="IOutboundEffectAdapter.ProviderId"/>. Duplicate provider-id
/// registrations throw at construction to avoid silent shadowing.
/// </summary>
public sealed class OutboundEffectAdapterRegistry : IOutboundEffectAdapterRegistry
{
    private readonly IReadOnlyDictionary<string, IOutboundEffectAdapter> _byProvider;

    public OutboundEffectAdapterRegistry(IEnumerable<IOutboundEffectAdapter> adapters)
    {
        ArgumentNullException.ThrowIfNull(adapters);
        var map = new Dictionary<string, IOutboundEffectAdapter>(StringComparer.Ordinal);
        foreach (var adapter in adapters)
        {
            if (map.ContainsKey(adapter.ProviderId))
                throw new InvalidOperationException(
                    $"Duplicate IOutboundEffectAdapter registration for ProviderId '{adapter.ProviderId}'.");
            map[adapter.ProviderId] = adapter;
        }
        _byProvider = map;
    }

    public bool TryGet(string providerId, out IOutboundEffectAdapter? adapter)
    {
        if (_byProvider.TryGetValue(providerId, out var match))
        {
            adapter = match;
            return true;
        }
        adapter = null;
        return false;
    }

    public IReadOnlyCollection<string> RegisteredProviders => (IReadOnlyCollection<string>)_byProvider.Keys;
}
