namespace Whycespace.Projections.Intelligence.Cost.CostBenchmark;

public interface ICostBenchmarkViewRepository
{
    Task SaveAsync(CostBenchmarkReadModel model, CancellationToken ct = default);
    Task<CostBenchmarkReadModel?> GetAsync(string id, CancellationToken ct = default);
}
