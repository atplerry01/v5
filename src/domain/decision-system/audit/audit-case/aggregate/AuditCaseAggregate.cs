namespace Whycespace.Domain.DecisionSystem.Audit.AuditCase;

public sealed class AuditCaseAggregate
{
    public static AuditCaseAggregate Create()
    {
        var aggregate = new AuditCaseAggregate();
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
