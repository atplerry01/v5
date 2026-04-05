namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed class CredentialAggregate
{
    public static CredentialAggregate Create()
    {
        var aggregate = new CredentialAggregate();
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
