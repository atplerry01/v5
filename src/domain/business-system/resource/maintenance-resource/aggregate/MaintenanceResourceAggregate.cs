namespace Whycespace.Domain.BusinessSystem.Resource.MaintenanceResource;

public sealed class MaintenanceResourceAggregate
{
    public static MaintenanceResourceAggregate Create()
    {
        var aggregate = new MaintenanceResourceAggregate();
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
