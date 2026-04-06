namespace Whycespace.Projections.Business.Portfolio.Performance;

public interface IPerformanceViewRepository
{
    Task SaveAsync(PerformanceReadModel model, CancellationToken ct = default);
    Task<PerformanceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
