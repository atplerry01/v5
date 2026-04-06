namespace Whycespace.Projections.Business.Portfolio.Benchmark;

public interface IBenchmarkViewRepository
{
    Task SaveAsync(BenchmarkReadModel model, CancellationToken ct = default);
    Task<BenchmarkReadModel?> GetAsync(string id, CancellationToken ct = default);
}
