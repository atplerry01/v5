namespace Whycespace.Domain.BusinessSystem.Agreement.Term;

public sealed class TermAggregate
{
    public static TermAggregate Create()
    {
        var aggregate = new TermAggregate();
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
