namespace Whycespace.Domain.IntelligenceSystem.Simulation.Scenario;

public sealed class ScenarioAggregate
{
    public static ScenarioAggregate Create()
    {
        var aggregate = new ScenarioAggregate();
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
