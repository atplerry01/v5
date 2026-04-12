namespace Whycespace.Domain.BusinessSystem.Scheduler.Availability;

public static class AvailabilityErrors
{
    public static AvailabilityDomainException MissingId()
        => new("AvailabilityId is required and must not be empty.");

    public static AvailabilityDomainException InvalidTimeRange()
        => new("Availability time range end must be after start.");

    public static AvailabilityDomainException AlreadyDeactivated(AvailabilityId id)
        => new($"Availability '{id.Value}' has already been deactivated.");

    public static AvailabilityDomainException InvalidStateTransition(AvailabilityStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class AvailabilityDomainException : Exception
{
    public AvailabilityDomainException(string message) : base(message) { }
}
