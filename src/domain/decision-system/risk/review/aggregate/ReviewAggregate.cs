namespace Whycespace.Domain.DecisionSystem.Risk.Review;

public sealed class ReviewAggregate
{
    public static ReviewAggregate Create()
    {
        var aggregate = new ReviewAggregate();
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
