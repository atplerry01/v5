namespace Whycespace.Domain.IntelligenceSystem.Simulation.Assumption;

public sealed class AssumptionAggregate
{
    public static AssumptionAggregate Create()
    {
        var aggregate = new AssumptionAggregate();
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
