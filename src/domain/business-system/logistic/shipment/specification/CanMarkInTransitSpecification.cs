namespace Whycespace.Domain.BusinessSystem.Logistic.Shipment;

public sealed class CanMarkInTransitSpecification
{
    public bool IsSatisfiedBy(ShipmentStatus status)
    {
        return status == ShipmentStatus.Dispatched;
    }
}
