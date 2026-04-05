namespace Whycespace.Domain.CoreSystem.Financialcontrol.ApprovalControl;

public sealed class ApprovalControlAggregate
{
    public static ApprovalControlAggregate Create()
    {
        var aggregate = new ApprovalControlAggregate();
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
