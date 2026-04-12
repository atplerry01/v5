namespace Whycespace.Domain.BusinessSystem.Logistic.Shipment;

public sealed class CanPackSpecification
{
    public bool IsSatisfiedBy(ShipmentStatus status)
    {
        return status == ShipmentStatus.Created;
    }
}
