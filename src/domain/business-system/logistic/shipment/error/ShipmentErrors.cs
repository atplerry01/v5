namespace Whycespace.Domain.BusinessSystem.Logistic.Shipment;

public static class ShipmentErrors
{
    public static ShipmentDomainException MissingId()
        => new("ShipmentId is required and must not be empty.");

    public static ShipmentDomainException OriginRequired()
        => new("Shipment must have an origin.");

    public static ShipmentDomainException DestinationRequired()
        => new("Shipment must have a destination.");

    public static ShipmentDomainException ItemReferenceRequired()
        => new("Shipment must reference at least one item.");

    public static ShipmentDomainException InvalidStateTransition(ShipmentStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ShipmentDomainException AlreadyDelivered()
        => new("Shipment has been delivered and cannot be modified.");
}

public sealed class ShipmentDomainException : Exception
{
    public ShipmentDomainException(string message) : base(message) { }
}
