namespace Whycespace.Domain.IntelligenceSystem.Simulation.Model;

public sealed class ModelAggregate
{
    public static ModelAggregate Create()
    {
        var aggregate = new ModelAggregate();
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
