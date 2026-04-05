namespace Whycespace.Domain.IntelligenceSystem.Capacity.Supply;

public sealed class SupplyAggregate
{
    public static SupplyAggregate Create()
    {
        var aggregate = new SupplyAggregate();
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
