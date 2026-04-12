namespace Whycespace.Domain.BusinessSystem.Logistic.Shipment;

public sealed class CanDispatchSpecification
{
    public bool IsSatisfiedBy(ShipmentStatus status)
    {
        return status == ShipmentStatus.Packed;
    }
}
