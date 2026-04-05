namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed class ProfileAggregate
{
    public static ProfileAggregate Create()
    {
        var aggregate = new ProfileAggregate();
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
