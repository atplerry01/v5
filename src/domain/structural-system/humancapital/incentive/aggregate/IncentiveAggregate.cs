namespace Whycespace.Domain.StructuralSystem.Humancapital.Incentive;

public sealed class IncentiveAggregate
{
    public static IncentiveAggregate Create()
    {
        var aggregate = new IncentiveAggregate();
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
