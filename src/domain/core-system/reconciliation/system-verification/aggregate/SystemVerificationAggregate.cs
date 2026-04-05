namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

public sealed class SystemVerificationAggregate
{
    public static SystemVerificationAggregate Create()
    {
        var aggregate = new SystemVerificationAggregate();
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
