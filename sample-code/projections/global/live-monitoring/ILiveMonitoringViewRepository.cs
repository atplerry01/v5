namespace Whycespace.Projections.Global.LiveMonitoring;

public interface ILiveMonitoringViewRepository
{
    Task SaveAsync(LiveMonitoringReadModel model, CancellationToken ct = default);
    Task<LiveMonitoringReadModel?> GetAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<LiveMonitoringReadModel>> GetByRegionAsync(string regionId, CancellationToken ct = default);
    Task<IReadOnlyList<LiveMonitoringReadModel>> GetCriticalAlertsAsync(CancellationToken ct = default);
}
