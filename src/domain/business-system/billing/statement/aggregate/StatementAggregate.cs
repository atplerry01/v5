namespace Whycespace.Domain.BusinessSystem.Billing.Statement;

public sealed class StatementAggregate
{
    public static StatementAggregate Create()
    {
        var aggregate = new StatementAggregate();
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
