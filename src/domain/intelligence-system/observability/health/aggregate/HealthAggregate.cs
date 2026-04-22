using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.IntelligenceSystem.Observability.Health;

public sealed class HealthAggregate : AggregateRoot
{
    public static HealthAggregate Create()
    {
        var aggregate = new HealthAggregate();
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
