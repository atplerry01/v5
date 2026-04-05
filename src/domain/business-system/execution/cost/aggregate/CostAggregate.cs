namespace Whycespace.Domain.BusinessSystem.Execution.Cost;

public sealed class CostAggregate
{
    public static CostAggregate Create()
    {
        var aggregate = new CostAggregate();
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
