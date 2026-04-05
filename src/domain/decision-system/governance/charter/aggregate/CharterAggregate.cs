namespace Whycespace.Domain.DecisionSystem.Governance.Charter;

public sealed class CharterAggregate
{
    public static CharterAggregate Create()
    {
        var aggregate = new CharterAggregate();
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
