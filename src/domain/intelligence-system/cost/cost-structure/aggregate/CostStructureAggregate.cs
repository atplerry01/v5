namespace Whycespace.Domain.IntelligenceSystem.Cost.CostStructure;

public sealed class CostStructureAggregate
{
    public static CostStructureAggregate Create()
    {
        var aggregate = new CostStructureAggregate();
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
