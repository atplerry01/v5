using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

public sealed class JurisdictionAggregate : AggregateRoot
{
    public static JurisdictionAggregate Create()
    {
        var aggregate = new JurisdictionAggregate();
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
