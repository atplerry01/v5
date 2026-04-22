using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.DecisionSystem.Risk.Control;

public sealed class ControlAggregate : AggregateRoot
{
    public static ControlAggregate Create()
    {
        var aggregate = new ControlAggregate();
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
