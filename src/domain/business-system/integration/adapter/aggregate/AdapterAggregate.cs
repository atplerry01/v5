namespace Whycespace.Domain.BusinessSystem.Integration.Adapter;

public sealed class AdapterAggregate
{
    public static AdapterAggregate Create()
    {
        var aggregate = new AdapterAggregate();
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
