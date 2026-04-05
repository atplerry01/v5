namespace Whycespace.Domain.IntelligenceSystem.Index.CostIndex;

public sealed class CostIndexAggregate
{
    public static CostIndexAggregate Create()
    {
        var aggregate = new CostIndexAggregate();
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
