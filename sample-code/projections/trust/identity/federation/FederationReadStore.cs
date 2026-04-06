using Whycespace.Projections.IdentityFederation.ReadModels;
using Whycespace.Shared.Contracts.Infrastructure;

namespace Whycespace.Projections.IdentityFederation.Queries;

/// <summary>
/// Read store for identity federation projections.
/// Typed wrapper around IProjectionStore.
/// </summary>
public sealed class FederationReadStore
{
    private const string Projection = "identity-federation";
    private readonly IProjectionStore _store;

    public FederationReadStore(IProjectionStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    // Issuer
    public Task<IssuerReadModel?> GetIssuerAsync(string issuerId, CancellationToken ct = default)
        => _store.GetAsync<IssuerReadModel>(Projection, $"issuer:{issuerId}", ct);

    public Task SetIssuerAsync(string issuerId, IssuerReadModel model, CancellationToken ct = default)
        => _store.SetAsync(Projection, $"issuer:{issuerId}", model, ct);

    // Issuer Trust
    public Task<IssuerTrustReadModel?> GetIssuerTrustAsync(string issuerId, CancellationToken ct = default)
        => _store.GetAsync<IssuerTrustReadModel>(Projection, $"trust:{issuerId}", ct);

    public Task SetIssuerTrustAsync(string issuerId, IssuerTrustReadModel model, CancellationToken ct = default)
        => _store.SetAsync(Projection, $"trust:{issuerId}", model, ct);

    // Federation Links (per identity)
    public Task<IReadOnlyList<FederationLinkReadModel>> GetLinksAsync(string identityId, CancellationToken ct = default)
        => _store.GetAllAsync<FederationLinkReadModel>($"{Projection}:links:{identityId}", ct);

    public Task SetLinkAsync(string identityId, string linkKey, FederationLinkReadModel model, CancellationToken ct = default)
        => _store.SetAsync($"{Projection}:links:{identityId}", linkKey, model, ct);

    public Task RemoveLinkAsync(string identityId, string linkKey, CancellationToken ct = default)
        => _store.RemoveAsync($"{Projection}:links:{identityId}", linkKey, ct);
}
