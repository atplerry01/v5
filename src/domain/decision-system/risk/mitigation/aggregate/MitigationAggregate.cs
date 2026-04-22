using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.DecisionSystem.Risk.Mitigation;

public sealed class MitigationAggregate : AggregateRoot
{
    public static MitigationAggregate Create()
    {
        var aggregate = new MitigationAggregate();
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
