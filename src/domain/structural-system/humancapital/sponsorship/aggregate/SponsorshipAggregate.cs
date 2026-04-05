namespace Whycespace.Domain.StructuralSystem.Humancapital.Sponsorship;

public sealed class SponsorshipAggregate
{
    public static SponsorshipAggregate Create()
    {
        var aggregate = new SponsorshipAggregate();
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
