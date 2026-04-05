namespace Whycespace.Domain.IntelligenceSystem.Capacity.Constraint;

public sealed class ConstraintAggregate
{
    public static ConstraintAggregate Create()
    {
        var aggregate = new ConstraintAggregate();
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
