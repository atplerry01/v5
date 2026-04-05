namespace Whycespace.Domain.DecisionSystem.Governance.ComplianceReview;

public sealed class ComplianceReviewAggregate
{
    public static ComplianceReviewAggregate Create()
    {
        var aggregate = new ComplianceReviewAggregate();
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
