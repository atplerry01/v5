namespace Whycespace.Projections.Intelligence.Simulation.Comparison;

public interface IComparisonViewRepository
{
    Task SaveAsync(ComparisonReadModel model, CancellationToken ct = default);
    Task<ComparisonReadModel?> GetAsync(string id, CancellationToken ct = default);
}
