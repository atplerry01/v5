using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.ConstitutionalSystem.Policy.Constraint;

public sealed class ConstraintAggregate : AggregateRoot
{
    public static ConstraintAggregate Create()
    {
        var aggregate = new ConstraintAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    protected override void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    protected override void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
