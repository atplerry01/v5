namespace Whycespace.Domain.DecisionSystem.Compliance.Filing;

public sealed class FilingAggregate
{
    public static FilingAggregate Create()
    {
        var aggregate = new FilingAggregate();
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
