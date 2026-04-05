namespace Whycespace.Domain.DecisionSystem.Risk.Rating;

public sealed class RatingAggregate
{
    public static RatingAggregate Create()
    {
        var aggregate = new RatingAggregate();
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
