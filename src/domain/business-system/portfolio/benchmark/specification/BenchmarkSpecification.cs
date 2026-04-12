namespace Whycespace.Domain.BusinessSystem.Portfolio.Benchmark;

public sealed class BenchmarkSpecification
{
    public bool IsSatisfiedBy(BenchmarkId id, BenchmarkName name)
    {
        return id.Value != Guid.Empty
            && !string.IsNullOrWhiteSpace(name.Value);
    }
}
