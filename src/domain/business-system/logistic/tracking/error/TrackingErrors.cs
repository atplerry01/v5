namespace Whycespace.Domain.BusinessSystem.Logistic.Tracking;

public static class TrackingErrors
{
    public static TrackingDomainException MissingId()
        => new("TrackingId is required and must not be empty.");

    public static TrackingDomainException ShipmentReferenceRequired()
        => new("Tracking must reference a shipment.");

    public static TrackingDomainException TrackingPointRequired()
        => new("Tracking must have at least one tracking point.");

    public static TrackingDomainException InvalidStateTransition(TrackingStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static TrackingDomainException AlreadyCompleted()
        => new("Tracking has been completed and cannot be modified.");
}

public sealed class TrackingDomainException : Exception
{
    public TrackingDomainException(string message) : base(message) { }
}
