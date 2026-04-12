namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public static class FulfillmentErrors
{
    public static FulfillmentDomainException MissingId()
        => new("FulfillmentId is required and must not be empty.");

    public static FulfillmentDomainException MissingShipmentReference()
        => new("ShipmentReference is required and must not be empty.");

    public static FulfillmentDomainException AlreadyCompleted()
        => new("Fulfillment has already been completed and cannot be completed again.");

    public static FulfillmentDomainException InvalidStatus(FulfillmentStatus status)
        => new($"FulfillmentStatus '{status}' is not valid for this operation.");
}

public sealed class FulfillmentDomainException : Exception
{
    public FulfillmentDomainException(string message) : base(message) { }
}
