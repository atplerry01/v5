namespace Whycespace.Domain.BusinessSystem.Logistic.Shipment;

public sealed class ShipmentAggregate
{
    public static ShipmentAggregate Create()
    {
        var aggregate = new ShipmentAggregate();
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
