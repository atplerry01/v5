namespace Whycespace.Domain.BusinessSystem.Inventory.Lot;

public sealed class LotAggregate
{
    public static LotAggregate Create()
    {
        var aggregate = new LotAggregate();
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
