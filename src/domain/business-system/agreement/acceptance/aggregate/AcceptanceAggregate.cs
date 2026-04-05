namespace Whycespace.Domain.BusinessSystem.Agreement.Acceptance;

public sealed class AcceptanceAggregate
{
    public static AcceptanceAggregate Create()
    {
        var aggregate = new AcceptanceAggregate();
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
