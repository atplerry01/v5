namespace Whycespace.Projections.Intelligence.Estimation.Benchmark;

public interface IBenchmarkViewRepository
{
    Task SaveAsync(BenchmarkReadModel model, CancellationToken ct = default);
    Task<BenchmarkReadModel?> GetAsync(string id, CancellationToken ct = default);
}
