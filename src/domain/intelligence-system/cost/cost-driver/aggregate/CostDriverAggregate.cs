namespace Whycespace.Domain.IntelligenceSystem.Cost.CostDriver;

public sealed class CostDriverAggregate
{
    public static CostDriverAggregate Create()
    {
        var aggregate = new CostDriverAggregate();
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
