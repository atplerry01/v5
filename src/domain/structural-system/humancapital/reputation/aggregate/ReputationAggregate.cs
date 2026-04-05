namespace Whycespace.Domain.StructuralSystem.Humancapital.Reputation;

public sealed class ReputationAggregate
{
    public static ReputationAggregate Create()
    {
        var aggregate = new ReputationAggregate();
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
