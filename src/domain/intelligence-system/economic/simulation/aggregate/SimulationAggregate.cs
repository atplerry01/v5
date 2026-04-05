namespace Whycespace.Domain.IntelligenceSystem.Economic.Simulation;

public sealed class SimulationAggregate
{
    public static SimulationAggregate Create()
    {
        var aggregate = new SimulationAggregate();
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
