namespace Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

public sealed class JurisdictionAggregate
{
    public static JurisdictionAggregate Create()
    {
        var aggregate = new JurisdictionAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    private void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
