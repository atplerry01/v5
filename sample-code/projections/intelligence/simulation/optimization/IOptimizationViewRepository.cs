namespace Whycespace.Projections.Intelligence.Simulation.Optimization;

public interface IOptimizationViewRepository
{
    Task SaveAsync(OptimizationReadModel model, CancellationToken ct = default);
    Task<OptimizationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
