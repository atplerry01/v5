namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public sealed class ReserveAggregate
{
    public static ReserveAggregate Create()
    {
        var aggregate = new ReserveAggregate();
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
