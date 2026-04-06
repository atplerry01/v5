namespace Whycespace.Projections.Intelligence.Economic.Optimization;

public interface IOptimizationViewRepository
{
    Task SaveAsync(OptimizationReadModel model, CancellationToken ct = default);
    Task<OptimizationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
