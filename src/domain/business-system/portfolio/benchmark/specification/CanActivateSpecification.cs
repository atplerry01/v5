namespace Whycespace.Domain.BusinessSystem.Portfolio.Benchmark;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(BenchmarkStatus status)
    {
        return status == BenchmarkStatus.Draft;
    }
}
