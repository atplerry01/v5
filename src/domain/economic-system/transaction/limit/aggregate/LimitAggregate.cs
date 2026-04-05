namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed class LimitAggregate
{
    public static LimitAggregate Create()
    {
        var aggregate = new LimitAggregate();
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
