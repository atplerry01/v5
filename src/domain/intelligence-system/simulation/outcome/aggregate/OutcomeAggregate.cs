namespace Whycespace.Domain.IntelligenceSystem.Simulation.Outcome;

public sealed class OutcomeAggregate
{
    public static OutcomeAggregate Create()
    {
        var aggregate = new OutcomeAggregate();
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
