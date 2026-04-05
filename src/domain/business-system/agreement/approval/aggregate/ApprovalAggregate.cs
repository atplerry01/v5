namespace Whycespace.Domain.BusinessSystem.Agreement.Approval;

public sealed class ApprovalAggregate
{
    public static ApprovalAggregate Create()
    {
        var aggregate = new ApprovalAggregate();
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
