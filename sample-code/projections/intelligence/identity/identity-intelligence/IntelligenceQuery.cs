using Whycespace.Projections.IdentityIntelligence.ReadModels;

namespace Whycespace.Projections.IdentityIntelligence.Queries;

/// <summary>
/// Read-only query facade for identity intelligence projections.
/// </summary>
public sealed class IntelligenceQuery
{
    private readonly IntelligenceReadStore _store;

    public IntelligenceQuery(IntelligenceReadStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Task<TrustScoreReadModel?> GetTrustScoreAsync(string identityId, CancellationToken ct = default)
        => _store.GetTrustAsync(identityId, ct);

    public Task<RiskScoreReadModel?> GetRiskScoreAsync(string identityId, CancellationToken ct = default)
        => _store.GetRiskAsync(identityId, ct);

    public Task<IdentityGraphReadModel?> GetGraphAsync(string identityId, CancellationToken ct = default)
        => _store.GetGraphAsync(identityId, ct);

    public Task<AnomalyReadModel?> GetAnomaliesAsync(string identityId, CancellationToken ct = default)
        => _store.GetAnomaliesAsync(identityId, ct);
}
