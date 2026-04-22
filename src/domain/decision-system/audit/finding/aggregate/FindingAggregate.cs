using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.DecisionSystem.Audit.Finding;

public sealed class FindingAggregate : AggregateRoot
{
    public static FindingAggregate Create()
    {
        var aggregate = new FindingAggregate();
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
