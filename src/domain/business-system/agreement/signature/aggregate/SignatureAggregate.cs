namespace Whycespace.Domain.BusinessSystem.Agreement.Signature;

public sealed class SignatureAggregate
{
    public static SignatureAggregate Create()
    {
        var aggregate = new SignatureAggregate();
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
