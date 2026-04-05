namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed class DeviceAggregate
{
    public static DeviceAggregate Create()
    {
        var aggregate = new DeviceAggregate();
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
