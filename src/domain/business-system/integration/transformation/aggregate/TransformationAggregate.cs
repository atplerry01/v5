namespace Whycespace.Domain.BusinessSystem.Integration.Transformation;

public sealed class TransformationAggregate
{
    public static TransformationAggregate Create()
    {
        var aggregate = new TransformationAggregate();
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
