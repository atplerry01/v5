namespace Whycespace.Projections.Global.AnomalyDashboard;

public interface IAnomalyDashboardViewRepository
{
    Task SaveAsync(AnomalyDashboardReadModel model, CancellationToken ct = default);
    Task<AnomalyDashboardReadModel?> GetAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<AnomalyDashboardReadModel>> GetUnresolvedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AnomalyDashboardReadModel>> GetByRegionAsync(string regionId, CancellationToken ct = default);
}
