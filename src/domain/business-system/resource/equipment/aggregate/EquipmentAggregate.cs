namespace Whycespace.Domain.BusinessSystem.Resource.Equipment;

public sealed class EquipmentAggregate
{
    public static EquipmentAggregate Create()
    {
        var aggregate = new EquipmentAggregate();
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
