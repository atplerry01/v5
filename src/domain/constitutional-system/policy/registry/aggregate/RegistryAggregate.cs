using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class RegistryAggregate : AggregateRoot
{
    public static RegistryAggregate Create()
    {
        var aggregate = new RegistryAggregate();
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
