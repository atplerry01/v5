namespace Whycespace.Domain.IntelligenceSystem.Search.Index;

public sealed class IndexAggregate
{
    public static IndexAggregate Create()
    {
        var aggregate = new IndexAggregate();
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
