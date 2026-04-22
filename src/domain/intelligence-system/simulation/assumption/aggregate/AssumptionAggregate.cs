using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.IntelligenceSystem.Simulation.Assumption;

public sealed class AssumptionAggregate : AggregateRoot
{
    public static AssumptionAggregate Create()
    {
        var aggregate = new AssumptionAggregate();
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
