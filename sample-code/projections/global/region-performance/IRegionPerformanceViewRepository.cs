namespace Whycespace.Projections.Global.RegionPerformance;

public interface IRegionPerformanceViewRepository
{
    Task SaveAsync(RegionPerformanceReadModel model, CancellationToken ct = default);
    Task<RegionPerformanceReadModel?> GetAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<RegionPerformanceReadModel>> GetAllAsync(CancellationToken ct = default);
}
