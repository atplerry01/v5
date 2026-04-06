namespace Whycespace.Projections.Intelligence.Index.PerformanceIndex;

public interface IPerformanceIndexViewRepository
{
    Task SaveAsync(PerformanceIndexReadModel model, CancellationToken ct = default);
    Task<PerformanceIndexReadModel?> GetAsync(string id, CancellationToken ct = default);
}
