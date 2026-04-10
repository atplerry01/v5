namespace Whycespace.Domain.Operational.Deployment.Emergency;

public sealed class EmergencyAggregate
{
    public static EmergencyAggregate Create()
    {
        var aggregate = new EmergencyAggregate();
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
