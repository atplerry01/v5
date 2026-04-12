namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed class FulfillmentSpecification
{
    public bool IsSatisfiedBy(FulfillmentId id, ShipmentReference shipmentReference)
    {
        return id != default && shipmentReference != default;
    }
}
