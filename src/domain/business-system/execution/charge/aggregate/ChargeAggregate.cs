namespace Whycespace.Domain.BusinessSystem.Execution.Charge;

public sealed class ChargeAggregate
{
    public static ChargeAggregate Create()
    {
        var aggregate = new ChargeAggregate();
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
