namespace Whycespace.Domain.DecisionSystem.Compliance.ComplianceCase;

public sealed class ComplianceCaseAggregate
{
    public static ComplianceCaseAggregate Create()
    {
        var aggregate = new ComplianceCaseAggregate();
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
