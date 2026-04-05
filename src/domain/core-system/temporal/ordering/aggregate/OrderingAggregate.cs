namespace Whycespace.Domain.CoreSystem.Temporal.Ordering;

public sealed class OrderingAggregate
{
    public static OrderingAggregate Create()
    {
        var aggregate = new OrderingAggregate();
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
