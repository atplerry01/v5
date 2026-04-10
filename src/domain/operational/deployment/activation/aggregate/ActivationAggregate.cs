namespace Whycespace.Domain.Operational.Deployment.Activation;

public sealed class ActivationAggregate
{
    public static ActivationAggregate Create()
    {
        var aggregate = new ActivationAggregate();
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
