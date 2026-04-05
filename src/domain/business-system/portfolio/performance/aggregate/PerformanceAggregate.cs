namespace Whycespace.Domain.BusinessSystem.Portfolio.Performance;

public sealed class PerformanceAggregate
{
    public static PerformanceAggregate Create()
    {
        var aggregate = new PerformanceAggregate();
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
