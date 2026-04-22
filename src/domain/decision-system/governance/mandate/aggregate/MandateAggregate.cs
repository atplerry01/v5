using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.DecisionSystem.Governance.Mandate;

public sealed class MandateAggregate : AggregateRoot
{
    public static MandateAggregate Create()
    {
        var aggregate = new MandateAggregate();
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
