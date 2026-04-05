namespace Whycespace.Domain.IntelligenceSystem.Search.Synonym;

public sealed class SynonymAggregate
{
    public static SynonymAggregate Create()
    {
        var aggregate = new SynonymAggregate();
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
