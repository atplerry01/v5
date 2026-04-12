namespace Whycespace.Domain.BusinessSystem.Portfolio.Benchmark;

public sealed class CanRetireSpecification
{
    public bool IsSatisfiedBy(BenchmarkStatus status)
    {
        return status == BenchmarkStatus.Active;
    }
}
