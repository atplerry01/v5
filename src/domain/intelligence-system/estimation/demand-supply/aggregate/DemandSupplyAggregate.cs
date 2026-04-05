namespace Whycespace.Domain.IntelligenceSystem.Estimation.DemandSupply;

public sealed class DemandSupplyAggregate
{
    public static DemandSupplyAggregate Create()
    {
        var aggregate = new DemandSupplyAggregate();
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
