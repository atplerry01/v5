using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.OrchestrationSystem.Workflow.Route;

public sealed class RouteAggregate : AggregateRoot
{
    public static RouteAggregate Create()
    {
        var aggregate = new RouteAggregate();
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
