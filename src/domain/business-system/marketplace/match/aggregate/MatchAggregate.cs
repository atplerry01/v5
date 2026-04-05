namespace Whycespace.Domain.BusinessSystem.Marketplace.Match;

public sealed class MatchAggregate
{
    public static MatchAggregate Create()
    {
        var aggregate = new MatchAggregate();
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
