namespace Whycespace.Domain.TrustSystem.Identity.Registry;

public sealed class RegistryAggregate
{
    public static RegistryAggregate Create()
    {
        var aggregate = new RegistryAggregate();
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
