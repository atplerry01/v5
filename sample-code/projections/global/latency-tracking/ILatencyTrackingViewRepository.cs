namespace Whycespace.Projections.Global.LatencyTracking;

public interface ILatencyTrackingViewRepository
{
    Task SaveAsync(LatencyTrackingReadModel model, CancellationToken ct = default);
    Task<LatencyTrackingReadModel?> GetAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<LatencyTrackingReadModel>> GetByRegionAsync(string regionId, CancellationToken ct = default);
    Task<IReadOnlyList<LatencyTrackingReadModel>> GetExceedingBudgetAsync(CancellationToken ct = default);
}
