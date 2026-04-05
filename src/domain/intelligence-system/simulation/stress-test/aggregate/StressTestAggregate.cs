namespace Whycespace.Domain.IntelligenceSystem.Simulation.StressTest;

public sealed class StressTestAggregate
{
    public static StressTestAggregate Create()
    {
        var aggregate = new StressTestAggregate();
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
