namespace Whycespace.Domain.StructuralSystem.Humancapital.Workforce;

public sealed class WorkforceAggregate
{
    public static WorkforceAggregate Create()
    {
        var aggregate = new WorkforceAggregate();
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
