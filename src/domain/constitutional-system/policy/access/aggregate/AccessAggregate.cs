using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.ConstitutionalSystem.Policy.Access;

public sealed class AccessAggregate : AggregateRoot
{
    public static AccessAggregate Create()
    {
        var aggregate = new AccessAggregate();
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
