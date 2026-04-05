namespace Whycespace.Domain.StructuralSystem.Humancapital.Operator;

public sealed class OperatorAggregate
{
    public static OperatorAggregate Create()
    {
        var aggregate = new OperatorAggregate();
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
