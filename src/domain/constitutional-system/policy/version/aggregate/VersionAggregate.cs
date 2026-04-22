using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.ConstitutionalSystem.Policy.Version;

public sealed class VersionAggregate : AggregateRoot
{
    public static VersionAggregate Create()
    {
        var aggregate = new VersionAggregate();
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
