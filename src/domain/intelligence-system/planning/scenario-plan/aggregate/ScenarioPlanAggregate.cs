namespace Whycespace.Domain.IntelligenceSystem.Planning.ScenarioPlan;

public sealed class ScenarioPlanAggregate
{
    public static ScenarioPlanAggregate Create()
    {
        var aggregate = new ScenarioPlanAggregate();
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
