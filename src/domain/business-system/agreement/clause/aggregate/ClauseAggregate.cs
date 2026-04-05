namespace Whycespace.Domain.BusinessSystem.Agreement.Clause;

public sealed class ClauseAggregate
{
    public static ClauseAggregate Create()
    {
        var aggregate = new ClauseAggregate();
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
