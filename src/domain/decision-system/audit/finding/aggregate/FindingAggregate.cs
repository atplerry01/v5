namespace Whycespace.Domain.DecisionSystem.Audit.Finding;

public sealed class FindingAggregate
{
    public static FindingAggregate Create()
    {
        var aggregate = new FindingAggregate();
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
