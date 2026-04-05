namespace Whycespace.Domain.IntelligenceSystem.Planning.Objective;

public sealed class ObjectiveAggregate
{
    public static ObjectiveAggregate Create()
    {
        var aggregate = new ObjectiveAggregate();
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
