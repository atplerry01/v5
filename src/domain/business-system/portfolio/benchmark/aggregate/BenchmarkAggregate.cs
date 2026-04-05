namespace Whycespace.Domain.BusinessSystem.Portfolio.Benchmark;

public sealed class BenchmarkAggregate
{
    public static BenchmarkAggregate Create()
    {
        var aggregate = new BenchmarkAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    private void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
