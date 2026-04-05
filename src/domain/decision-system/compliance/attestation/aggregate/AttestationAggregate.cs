namespace Whycespace.Domain.DecisionSystem.Compliance.Attestation;

public sealed class AttestationAggregate
{
    public static AttestationAggregate Create()
    {
        var aggregate = new AttestationAggregate();
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
