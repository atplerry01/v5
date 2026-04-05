namespace Whycespace.Domain.BusinessSystem.Inventory.Movement;

public sealed class MovementAggregate
{
    public static MovementAggregate Create()
    {
        var aggregate = new MovementAggregate();
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
