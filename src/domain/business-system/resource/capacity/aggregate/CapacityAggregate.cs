namespace Whycespace.Domain.BusinessSystem.Resource.Capacity;

public sealed class CapacityAggregate
{
    public static CapacityAggregate Create()
    {
        var aggregate = new CapacityAggregate();
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
