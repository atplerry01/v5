namespace Whycespace.Domain.BusinessSystem.Integration.Secret;

public sealed class SecretAggregate
{
    public static SecretAggregate Create()
    {
        var aggregate = new SecretAggregate();
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
