using Whycespace.Projections.IdentityIntelligence.ReadModels;
using Whycespace.Shared.Contracts.Infrastructure;

namespace Whycespace.Projections.IdentityIntelligence.Queries;

/// <summary>
/// Read store for identity intelligence projections.
/// Typed wrapper around IProjectionStore.
/// </summary>
public sealed class IntelligenceReadStore
{
    private const string Projection = "identity-intelligence";
    private readonly IProjectionStore _store;

    public IntelligenceReadStore(IProjectionStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    // Trust
    public Task<TrustScoreReadModel?> GetTrustAsync(string identityId, CancellationToken ct = default)
        => _store.GetAsync<TrustScoreReadModel>(Projection, $"trust:{identityId}", ct);

    public Task SetTrustAsync(string identityId, TrustScoreReadModel model, CancellationToken ct = default)
        => _store.SetAsync(Projection, $"trust:{identityId}", model, ct);

    // Risk
    public Task<RiskScoreReadModel?> GetRiskAsync(string identityId, CancellationToken ct = default)
        => _store.GetAsync<RiskScoreReadModel>(Projection, $"risk:{identityId}", ct);

    public Task SetRiskAsync(string identityId, RiskScoreReadModel model, CancellationToken ct = default)
        => _store.SetAsync(Projection, $"risk:{identityId}", model, ct);

    // Graph
    public Task<IdentityGraphReadModel?> GetGraphAsync(string identityId, CancellationToken ct = default)
        => _store.GetAsync<IdentityGraphReadModel>(Projection, $"graph:{identityId}", ct);

    public Task SetGraphAsync(string identityId, IdentityGraphReadModel model, CancellationToken ct = default)
        => _store.SetAsync(Projection, $"graph:{identityId}", model, ct);

    // Anomalies
    public Task<AnomalyReadModel?> GetAnomaliesAsync(string identityId, CancellationToken ct = default)
        => _store.GetAsync<AnomalyReadModel>(Projection, $"anomaly:{identityId}", ct);

    public Task SetAnomaliesAsync(string identityId, AnomalyReadModel model, CancellationToken ct = default)
        => _store.SetAsync(Projection, $"anomaly:{identityId}", model, ct);

    public async Task ClearAllAsync(CancellationToken ct = default)
    {
        await Task.CompletedTask;
    }
}
