namespace Whycespace.Domain.IntelligenceSystem.Capacity.Demand;

public sealed class DemandAggregate
{
    public static DemandAggregate Create()
    {
        var aggregate = new DemandAggregate();
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
