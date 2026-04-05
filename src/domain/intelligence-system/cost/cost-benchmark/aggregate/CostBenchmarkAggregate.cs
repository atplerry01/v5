namespace Whycespace.Domain.IntelligenceSystem.Cost.CostBenchmark;

public sealed class CostBenchmarkAggregate
{
    public static CostBenchmarkAggregate Create()
    {
        var aggregate = new CostBenchmarkAggregate();
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
