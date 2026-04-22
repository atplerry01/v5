using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.DecisionSystem.Compliance.Obligation;

public sealed class ObligationAggregate : AggregateRoot
{
    public static ObligationAggregate Create()
    {
        var aggregate = new ObligationAggregate();
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
