namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — per-host registry of outbound-effect adapters keyed by
/// <c>IOutboundEffectAdapter.ProviderId</c>. Provided by DI composition;
/// resolves the adapter the relay uses for each queued effect.
/// </summary>
public interface IOutboundEffectAdapterRegistry
{
    bool TryGet(string providerId, out IOutboundEffectAdapter? adapter);

    IReadOnlyCollection<string> RegisteredProviders { get; }
}
