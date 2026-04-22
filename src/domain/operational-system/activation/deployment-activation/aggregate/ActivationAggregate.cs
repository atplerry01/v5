using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.OperationalSystem.Activation.DeploymentActivation;

public sealed class ActivationAggregate : AggregateRoot
{
    public static ActivationAggregate Create()
    {
        var aggregate = new ActivationAggregate();
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
