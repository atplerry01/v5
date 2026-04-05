namespace Whycespace.Domain.CoreSystem.Financialcontrol.BudgetControl;

public sealed class BudgetControlAggregate
{
    public static BudgetControlAggregate Create()
    {
        var aggregate = new BudgetControlAggregate();
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
