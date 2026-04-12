namespace Whycespace.Domain.BusinessSystem.Logistic.Tracking;

public sealed record TrackingCreatedEvent(
    TrackingId TrackingId,
    ShipmentReference ShipmentReference,
    TrackingPoint InitialPoint);
