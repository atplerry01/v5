namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed class ConsentAggregate
{
    public static ConsentAggregate Create()
    {
        var aggregate = new ConsentAggregate();
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
